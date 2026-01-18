using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using Source.Configurations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Source.Services.CdnService;

public class CdnService : ICdnService
{
    private readonly CdnSettings _settings;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<CdnService> _logger;

    public CdnService(IOptions<CdnSettings> options, ILogger<CdnService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint.Replace("http://", "").Replace("https://", ""))
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();
    }

    public async Task InitializeBucketAsync()
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_settings.BucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_settings.BucketName));
                
                _logger.LogInformation("Bucket created: {BucketName}", _settings.BucketName);

                // Set public read policy
                var policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": {"AWS": ["*"]},
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{_settings.BucketName}}/*"]
                        }
                    ]
                }
                """;

                await _minioClient.SetPolicyAsync(
                    new SetPolicyArgs()
                        .WithBucket(_settings.BucketName)
                        .WithPolicy(policy));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing bucket");
            throw;
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string? folder = null, bool generateThumbnail = false)
    {
        if (!ValidateFile(file, out string errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var objectName = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;

            using var stream = file.OpenReadStream();
            
            // Check if it's an image for optimization
            if (IsImageFile(extension))
            {
                using var image = await Image.LoadAsync(stream);
                
                // Resize if too large
                if (image.Width > _settings.MaxImageWidth || image.Height > _settings.MaxImageHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(_settings.MaxImageWidth, _settings.MaxImageHeight),
                        Mode = ResizeMode.Max
                    }));
                }

                using var optimizedStream = new MemoryStream();
                await image.SaveAsJpegAsync(optimizedStream, new JpegEncoder { Quality = _settings.JpegQuality });
                optimizedStream.Position = 0;

                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_settings.BucketName)
                        .WithObject(objectName)
                        .WithStreamData(optimizedStream)
                        .WithObjectSize(optimizedStream.Length)
                        .WithContentType(file.ContentType));

                // Generate thumbnail if requested
                if (generateThumbnail)
                {
                    await GenerateThumbnailAsync(image, objectName);
                }
            }
            else
            {
                // Upload non-image files directly
                stream.Position = 0;
                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_settings.BucketName)
                        .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithObjectSize(stream.Length)
                        .WithContentType(file.ContentType));
            }

            _logger.LogInformation("File uploaded: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            throw;
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var objectName = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;

            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType));

            _logger.LogInformation("File uploaded from stream: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file from stream");
            throw;
        }
    }

    public async Task<(byte[] FileData, string ContentType, string FileName)?> GetFileAsync(string fileName)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            
            var stat = await _minioClient.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(fileName));

            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(fileName)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

            return (memoryStream.ToArray(), stat.ContentType, Path.GetFileName(fileName));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File not found: {FileName}", fileName);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(fileName));

            _logger.LogInformation("File deleted: {FileName}", fileName);
            
            // Also delete thumbnail if exists
            var thumbnailName = GetThumbnailPath(fileName);
            try
            {
                await _minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(_settings.BucketName)
                        .WithObject(thumbnailName));
            }
            catch
            {
                // Ignore if thumbnail doesn't exist
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return false;
        }
    }

    public async Task<List<string>> ListFilesAsync(string? folder = null)
    {
        try
        {
            var files = new List<string>();
            var prefix = folder != null ? $"{folder}/" : string.Empty;

            var args = new ListObjectsArgs()
                .WithBucket(_settings.BucketName)
                .WithPrefix(prefix)
                .WithRecursive(true);

            await foreach (var item in _minioClient.ListObjectsEnumAsync(args))
            {
                files.Add(item.Key);
            }

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            throw;
        }
    }

    public async Task<string?> ResizeImageAsync(string fileName, int width, int height, string? outputFolder = null)
    {
        try
        {
            var fileData = await GetFileAsync(fileName);
            if (fileData == null) return null;

            using var inputStream = new MemoryStream(fileData.Value.FileData);
            using var image = await Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(width, height));

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_resized{extension}";
            var objectName = outputFolder != null ? $"{outputFolder}/{uniqueFileName}" : uniqueFileName;

            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = _settings.JpegQuality });
            outputStream.Position = 0;

            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(outputStream)
                    .WithObjectSize(outputStream.Length)
                    .WithContentType("image/jpeg"));

            _logger.LogInformation("Image resized: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return null;
        }
    }

    public string GetFileUrl(string fileName)
    {
        return $"{_settings.PublicUrl}/{_settings.BucketName}/{fileName}";
    }

    public bool ValidateFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "File is empty or not provided";
            return false;
        }

        if (file.Length > _settings.MaxFileSize)
        {
            errorMessage = $"File size exceeds maximum allowed size of {_settings.MaxFileSize / 1024 / 1024}MB";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            errorMessage = $"File extension {extension} is not allowed";
            return false;
        }

        return true;
    }

    private async Task GenerateThumbnailAsync(Image image, string originalFileName)
    {
        try
        {
            var thumbnail = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(_settings.ThumbnailWidth, _settings.ThumbnailHeight),
                Mode = ResizeMode.Crop
            }));

            var thumbnailName = GetThumbnailPath(originalFileName);

            using var thumbnailStream = new MemoryStream();
            await thumbnail.SaveAsJpegAsync(thumbnailStream, new JpegEncoder { Quality = 75 });
            thumbnailStream.Position = 0;

            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(thumbnailName)
                    .WithStreamData(thumbnailStream)
                    .WithObjectSize(thumbnailStream.Length)
                    .WithContentType("image/jpeg"));

            _logger.LogInformation("Thumbnail generated: {ThumbnailName}", thumbnailName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail");
        }
    }

    private string GetThumbnailPath(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        
        return directory != null 
            ? $"{directory}/thumbnails/{fileNameWithoutExt}_thumb{extension}"
            : $"thumbnails/{fileNameWithoutExt}_thumb{extension}";
    }

    private bool IsImageFile(string extension)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        return imageExtensions.Contains(extension.ToLowerInvariant());
    }
}

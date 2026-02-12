using Api.Application.Common.Interfaces;
using Api.Application.Media.Repositories;
using Api.Core.Domain;
using Api.Infrastructure.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Api.Infrastructure.Storage;

public class MediaService : IMediaService
{
    private readonly MediaSettings _settings;
    private readonly IMinioClient _minioClient;
    private readonly IMediaRepository _mediaRepository;

    public MediaService(
        IOptions<MediaSettings> options,
        IMediaRepository mediaRepository)
    {
        _settings = options.Value;
        _mediaRepository = mediaRepository;

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

                Log.Information("Bucket created: {BucketName}", _settings.BucketName);

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
            Log.Error(ex, "Error initializing bucket");
            throw;
        }
    }

    public async Task<(Guid Id, string FileName)> UploadFileAsync(IFormFile file, string? folder = null, bool generateThumbnail = false)
    {
        if (!ValidateFile(file, out string errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            var mediaId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{mediaId}{extension}";
            var objectName = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;

            int? width = null;
            int? height = null;
            string? thumbnailUrl = null;
            long fileSize = file.Length;

            using var stream = file.OpenReadStream();

            // Check if it's an image for optimization
            if (IsImageFile(extension))
            {
                using var image = await Image.LoadAsync(stream);
                width = image.Width;
                height = image.Height;

                // Resize if too large
                if (image.Width > _settings.MaxImageWidth || image.Height > _settings.MaxImageHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(_settings.MaxImageWidth, _settings.MaxImageHeight),
                        Mode = ResizeMode.Max
                    }));
                    width = image.Width;
                    height = image.Height;
                }

                using var optimizedStream = new MemoryStream();
                await image.SaveAsJpegAsync(optimizedStream, new JpegEncoder { Quality = _settings.JpegQuality });
                optimizedStream.Position = 0;
                fileSize = optimizedStream.Length;

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
                    var thumbPath = await GenerateThumbnailAsync(image, objectName);
                    thumbnailUrl = GetFileUrl(thumbPath);
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

            // Save to database
            var mediaModel = new MediaModel
            {
                Id = mediaId,
                FileName = objectName,
                OriginalName = file.FileName,
                ContentType = file.ContentType,
                FileSize = fileSize,
                Folder = folder,
                Url = GetFileUrl(objectName),
                ThumbnailUrl = thumbnailUrl,
                FileExtension = extension,
                IsImage = IsImageFile(extension),
                Width = width,
                Height = height,
                UploadedAt = DateTimeOffset.UtcNow
            };

            await _mediaRepository.InsertAsync(mediaModel);

            Log.Information("File uploaded: {ObjectName} with ID: {Id}", objectName, mediaId);
            return (mediaId, objectName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error uploading file");
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

            Log.Information("File uploaded from stream: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error uploading file from stream");
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
            Log.Warning(ex, "File not found: {FileName}", fileName);
            return null;
        }
    }

    public async Task<MediaModel?> GetFileByIdAsync(Guid id)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                Log.Warning("Media not found with ID: {Id}", id);
                return null;
            }

            return media;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting file by ID: {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(Guid id)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                Log.Warning("Media not found for deletion: {Id}", id);
                return false;
            }

            // Delete from MinIO
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(media.FileName));

            Log.Information("File deleted from MinIO: {FileName}", media.FileName);

            // Delete thumbnail if exists
            if (!string.IsNullOrEmpty(media.ThumbnailUrl))
            {
                var thumbnailName = GetThumbnailPath(media.FileName);
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
            }

            // Delete from database
            await _mediaRepository.DeleteAsync(id);
            Log.Information("Media deleted: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting media: {Id}", id);
            return false;
        }
    }

    public async Task<List<MediaModel>> ListFilesAsync(string? folder = null)
    {
        try
        {
            if (folder != null)
            {
                var folderFiles = await _mediaRepository.GetByFolderAsync(folder);
                return folderFiles.ToList();
            }
            else
            {
                var allFiles = await _mediaRepository.GetAllAsync();
                return allFiles.ToList();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error listing files");
            throw;
        }
    }

    public async Task<string?> ResizeImageAsync(Guid id, int width, int height, string? outputFolder = null)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                Log.Warning("Media not found with ID: {Id}", id);
                return null;
            }

            var fileData = await GetFileAsync(media.FileName);
            if (fileData == null) return null;

            using var inputStream = new MemoryStream(fileData.Value.FileData);
            using var image = await Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(width, height));

            var extension = Path.GetExtension(media.FileName);
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

            Log.Information("Image resized: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resizing image");
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

    private async Task<string> GenerateThumbnailAsync(Image image, string originalFileName)
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

            Log.Information("Thumbnail generated: {ThumbnailName}", thumbnailName);
            return thumbnailName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating thumbnail");
            throw;
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

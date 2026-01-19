using Microsoft.AspNetCore.Mvc;
using Source.Services.MediaService;
using Source.DTOs;
using Serilog;

namespace Source.Controllers;

[ApiController]
[Route("media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResponseDto>> UploadFile(
        IFormFile file,
        [FromQuery] string? folder = null,
        [FromQuery] bool generateThumbnail = false)
    {
        if (!_mediaService.ValidateFile(file, out string errorMessage))
        {
            return BadRequest(new UploadResponseDto
            {
                Success = false,
                Message = errorMessage
            });
        }

        var (id, fileName) = await _mediaService.UploadFileAsync(file, folder, generateThumbnail);
        var url = _mediaService.GetFileUrl(fileName);

        string? thumbnailUrl = null;
        if (generateThumbnail)
        {
            var directory = Path.GetDirectoryName(fileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var thumbnailPath = directory != null
                ? $"{directory}/thumbnails/{fileNameWithoutExt}_thumb{extension}"
                : $"thumbnails/{fileNameWithoutExt}_thumb{extension}";
            thumbnailUrl = _mediaService.GetFileUrl(thumbnailPath);
        }

        return Ok(new UploadResponseDto
        {
            Success = true,
            Id = id,
            FileName = fileName,
            Url = url,
            ThumbnailUrl = thumbnailUrl,
            FileSize = file.Length,
            Message = "File uploaded successfully"
        });
    }

    [HttpPost("upload/multiple")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MultipleUploadResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MultipleUploadResponseDto>> UploadMultipleFiles(
        List<IFormFile> files,
        [FromQuery] string? folder = null,
        [FromQuery] bool generateThumbnails = false)
    {
        var response = new MultipleUploadResponseDto();

        foreach (var file in files)
        {
            try
            {
                if (!_mediaService.ValidateFile(file, out string errorMessage))
                {
                    response.Results.Add(new UploadResponseDto
                    {
                        Success = false,
                        Message = errorMessage,
                        FileName = file.FileName
                    });
                    response.FailureCount++;
                    continue;
                }

                var (id, fileName) = await _mediaService.UploadFileAsync(file, folder, generateThumbnails);
                var url = _mediaService.GetFileUrl(fileName);

                response.Results.Add(new UploadResponseDto
                {
                    Success = true,
                    Id = id,
                    FileName = fileName,
                    Url = url,
                    FileSize = file.Length,
                    Message = "File uploaded successfully"
                });
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error uploading file {FileName}", file.FileName);
                response.Results.Add(new UploadResponseDto
                {
                    Success = false,
                    FileName = file.FileName,
                    Message = $"Error uploading {file.FileName}"
                });
                response.FailureCount++;
            }
        }

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResponseDto>> DeleteFile(Guid id)
    {
        var success = await _mediaService.DeleteFileAsync(id);

        if (!success)
        {
            return NotFound(new DeleteResponseDto
            {
                Success = false,
                Message = "File not found"
            });
        }

        return Ok(new DeleteResponseDto
        {
            Success = true,
            Message = "File deleted successfully"
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileById(Guid id)
    {
        var mediaModel = await _mediaService.GetFileByIdAsync(id);

        if (mediaModel == null)
        {
            return NotFound(new { message = "File not found" });
        }

        var result = await _mediaService.GetFileAsync(mediaModel.FileName);

        if (!result.HasValue)
        {
            return NotFound(new { message = "File not found in storage" });
        }

        var (fileData, contentType, originalFileName) = result.Value;
        return File(fileData, contentType, originalFileName);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(ListFilesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListFilesResponseDto>> ListFiles([FromQuery] string? folder = null)
    {
        var files = await _mediaService.ListFilesAsync(folder);

        var fileInfos = files.Select(f => new FileInfoDto
        {
            Id = f.Id,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Url = f.Url,
            ThumbnailUrl = f.ThumbnailUrl,
            Size = f.FileSize,
            ContentType = f.ContentType,
            IsImage = f.IsImage,
            Width = f.Width,
            Height = f.Height,
            UploadedAt = f.UploadedAt
        }).ToList();

        return Ok(new ListFilesResponseDto
        {
            Files = fileInfos,
            TotalCount = fileInfos.Count,
            Folder = folder
        });
    }

    [HttpPost("resize")]
    [ProducesResponseType(typeof(ResizeImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResizeImageResponseDto>> ResizeImage([FromBody] ResizeImageRequestDto request)
    {
        if (request.Width <= 0 || request.Height <= 0)
        {
            return BadRequest(new ResizeImageResponseDto
            {
                Success = false,
                Message = "Width and height must be greater than 0"
            });
        }

        var resizedFileName = await _mediaService.ResizeImageAsync(
            request.Id,
            request.Width,
            request.Height,
            request.OutputFolder);

        if (resizedFileName == null)
        {
            return NotFound(new ResizeImageResponseDto
            {
                Success = false,
                Message = "Original file not found or error during resizing"
            });
        }

        return Ok(new ResizeImageResponseDto
        {
            Success = true,
            ResizedFileName = resizedFileName,
            Url = _mediaService.GetFileUrl(resizedFileName),
            Message = "Image resized successfully"
        });
    }
}

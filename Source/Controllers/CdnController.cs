using Microsoft.AspNetCore.Mvc;
using Source.Services.CdnService;
using Source.DTOs;

namespace Source.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CdnController : ControllerBase
{
    private readonly ICdnService _cdnService;
    private readonly ILogger<CdnController> _logger;

    public CdnController(ICdnService cdnService, ILogger<CdnController> logger)
    {
        _cdnService = cdnService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single file to CDN
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResponseDto>> UploadFile(
        IFormFile file, 
        [FromQuery] string? folder = null,
        [FromQuery] bool generateThumbnail = false)
    {
        try
        {
            if (!_cdnService.ValidateFile(file, out string errorMessage))
            {
                return BadRequest(new UploadResponseDto
                {
                    Success = false,
                    Message = errorMessage
                });
            }

            var fileName = await _cdnService.UploadFileAsync(file, folder, generateThumbnail);
            var url = _cdnService.GetFileUrl(fileName);
            
            string? thumbnailUrl = null;
            if (generateThumbnail)
            {
                var directory = Path.GetDirectoryName(fileName);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var thumbnailPath = directory != null 
                    ? $"{directory}/thumbnails/{fileNameWithoutExt}_thumb{extension}"
                    : $"thumbnails/{fileNameWithoutExt}_thumb{extension}";
                thumbnailUrl = _cdnService.GetFileUrl(thumbnailPath);
            }

            return Ok(new UploadResponseDto
            {
                Success = true,
                FileName = fileName,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
                FileSize = file.Length,
                Message = "File uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return BadRequest(new UploadResponseDto
            {
                Success = false,
                Message = "An error occurred while uploading the file"
            });
        }
    }

    /// <summary>
    /// Upload multiple files to CDN
    /// </summary>
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
                if (!_cdnService.ValidateFile(file, out string errorMessage))
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

                var fileName = await _cdnService.UploadFileAsync(file, folder, generateThumbnails);
                var url = _cdnService.GetFileUrl(fileName);

                response.Results.Add(new UploadResponseDto
                {
                    Success = true,
                    FileName = fileName,
                    Url = url,
                    FileSize = file.Length,
                    Message = "File uploaded successfully"
                });
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
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

    /// <summary>
    /// Get a file from CDN
    /// </summary>
    [HttpGet("{**fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(string fileName)
    {
        try
        {
            var result = await _cdnService.GetFileAsync(fileName);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            var (fileData, contentType, originalFileName) = result.Value;
            return File(fileData, contentType, originalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileName}", fileName);
            return StatusCode(500, new { message = "An error occurred while retrieving the file" });
        }
    }

    /// <summary>
    /// Delete a file from CDN
    /// </summary>
    [HttpDelete("{**fileName}")]
    [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResponseDto>> DeleteFile(string fileName)
    {
        try
        {
            var success = await _cdnService.DeleteFileAsync(fileName);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return StatusCode(500, new DeleteResponseDto
            {
                Success = false,
                Message = "An error occurred while deleting the file"
            });
        }
    }

    /// <summary>
    /// List all files in CDN
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ListFilesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListFilesResponseDto>> ListFiles([FromQuery] string? folder = null)
    {
        try
        {
            var files = await _cdnService.ListFilesAsync(folder);

            var fileInfos = files.Select(f => new FileInfoDto
            {
                FileName = f,
                Url = _cdnService.GetFileUrl(f),
                Size = 0
            }).ToList();

            return Ok(new ListFilesResponseDto
            {
                Files = fileInfos,
                TotalCount = fileInfos.Count,
                Folder = folder
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return StatusCode(500, new { message = "An error occurred while listing files" });
        }
    }

    /// <summary>
    /// Resize an image
    /// </summary>
    [HttpPost("resize")]
    [ProducesResponseType(typeof(ResizeImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResizeImageResponseDto>> ResizeImage([FromBody] ResizeImageRequestDto request)
    {
        try
        {
            if (request.Width <= 0 || request.Height <= 0)
            {
                return BadRequest(new ResizeImageResponseDto
                {
                    Success = false,
                    Message = "Width and height must be greater than 0"
                });
            }

            var resizedFileName = await _cdnService.ResizeImageAsync(
                request.FileName, 
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
                Url = _cdnService.GetFileUrl(resizedFileName),
                Message = "Image resized successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return StatusCode(500, new ResizeImageResponseDto
            {
                Success = false,
                Message = "An error occurred while resizing the image"
            });
        }
    }
}

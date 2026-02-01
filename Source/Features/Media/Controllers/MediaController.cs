using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Source.Services.MediaService;
using Source.Features.Media.DTOs;
using Source.Common;

namespace Source.Features.Media.Controllers;

/// <summary>
/// Media file management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// Uploads a media file
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<UploadResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Upload([FromForm] UploadFileRequestDto request)
    {
        var file = request.File;
        var folder = request.Folder;
        if (file == null)
            return BadRequest(new ApiResponse<object>(message: "File is required.", errors: new List<string>{"File is required."}, success: false));
        if (!_mediaService.ValidateFile(file, out var error))
            return BadRequest(new ApiResponse<object>(message: error, errors: new List<string>{error}, success: false));
        var (id, fileName) = await _mediaService.UploadFileAsync(file, folder);
        var url = _mediaService.GetFileUrl(fileName);
        var responseDto = new UploadResponseDto { Success = true, Id = id, FileName = fileName, Url = url };
        return Ok(new ApiResponse<UploadResponseDto>(responseDto, "File uploaded successfully."));
    }

    /// <summary>
    /// Lists media files
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<ListFilesResponseDto>), 200)]
    public async Task<IActionResult> List([FromQuery] string? folder = null)
    {
        var files = await _mediaService.ListFilesAsync(folder);
        var result = files.Select(f => new FileInfoDto {
            Id = f.Id,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Url = f.Url,
            Size = f.FileSize,
            UploadedAt = f.UploadedAt,
            ContentType = f.ContentType,
            ThumbnailUrl = f.ThumbnailUrl,
            IsImage = f.IsImage,
            Width = f.Width,
            Height = f.Height
        }).ToList();
        var responseDto = new ListFilesResponseDto { Files = result };
        return Ok(new ApiResponse<ListFilesResponseDto>(responseDto, "Files listed successfully."));
    }

    /// <summary>
    /// Gets a media file by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<FileInfoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var file = await _mediaService.GetFileByIdAsync(id);
        if (file == null)
            return NotFound(new ApiResponse<object>(message: "File not found.", errors: new List<string>{"File not found."}, success: false));
        var dto = new FileInfoDto {
            Id = file.Id,
            FileName = file.FileName,
            OriginalName = file.OriginalName,
            Url = file.Url,
            Size = file.FileSize,
            UploadedAt = file.UploadedAt,
            ContentType = file.ContentType,
            ThumbnailUrl = file.ThumbnailUrl,
            IsImage = file.IsImage,
            Width = file.Width,
            Height = file.Height
        };
        return Ok(new ApiResponse<FileInfoDto>(dto, "File fetched successfully."));
    }

    /// <summary>
    /// Deletes a media file by id
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediaService.DeleteFileAsync(id);
        if (!success)
            return NotFound(new ApiResponse<object>(message: "File not found.", errors: new List<string>{"File not found."}, success: false));
        var responseDto = new DeleteResponseDto { Success = true, Message = "File deleted." };
        return Ok(new ApiResponse<DeleteResponseDto>(responseDto, "File deleted successfully."));
    }
}

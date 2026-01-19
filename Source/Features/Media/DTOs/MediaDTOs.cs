using System.ComponentModel.DataAnnotations;

namespace Source.Features.Media.DTOs;

// Upload Response DTO
public class UploadResponseDto
{
    public bool Success { get; set; }
    public Guid? Id { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Message { get; set; }
    public long? FileSize { get; set; }
}

// Multiple Upload Response DTO
public class MultipleUploadResponseDto
{
    public List<UploadResponseDto> Results { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

// File Info DTO
public class FileInfoDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTimeOffset? UploadedAt { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsImage { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

// Delete Response DTO
public class DeleteResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// List Files Response DTO
public class ListFilesResponseDto
{
    public List<FileInfoDto> Files { get; set; } = new();
}

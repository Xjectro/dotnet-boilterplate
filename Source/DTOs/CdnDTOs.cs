namespace Source.DTOs;

// Upload Response DTO
public class UploadResponseDto
{
    public bool Success { get; set; }
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
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public string ContentType { get; set; } = string.Empty;
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
    public int TotalCount { get; set; }
    public string? Folder { get; set; }
}

// Resize Image Request DTO
public class ResizeImageRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? OutputFolder { get; set; }
}

// Resize Image Response DTO
public class ResizeImageResponseDto
{
    public bool Success { get; set; }
    public string? ResizedFileName { get; set; }
    public string? Url { get; set; }
    public string? Message { get; set; }
}

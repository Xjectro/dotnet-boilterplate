namespace Source.Services.CdnService;

public interface ICdnService
{
    Task<string> UploadFileAsync(IFormFile file, string? folder = null, bool generateThumbnail = false);
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
    Task<(byte[] FileData, string ContentType, string FileName)?> GetFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
    Task<List<string>> ListFilesAsync(string? folder = null);
    Task<string?> ResizeImageAsync(string fileName, int width, int height, string? outputFolder = null);
    string GetFileUrl(string fileName);
    bool ValidateFile(IFormFile file, out string errorMessage);
    Task InitializeBucketAsync();
}

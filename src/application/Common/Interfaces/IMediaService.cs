using Api.Core.Domain;
using Microsoft.AspNetCore.Http;

namespace Api.Application.Common.Interfaces;

public interface IMediaService
{
    Task<(Guid Id, string FileName)> UploadFileAsync(IFormFile file, string? folder = null, bool generateThumbnail = false);
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
    Task<(byte[] FileData, string ContentType, string FileName)?> GetFileAsync(string fileName);
    Task<MediaModel?> GetFileByIdAsync(Guid id);
    Task<bool> DeleteFileAsync(Guid id);
    Task<List<MediaModel>> ListFilesAsync(string? folder = null);
    Task<string?> ResizeImageAsync(Guid id, int width, int height, string? outputFolder = null);
    string GetFileUrl(string fileName);
    bool ValidateFile(IFormFile file, out string errorMessage);
    Task InitializeBucketAsync();
}

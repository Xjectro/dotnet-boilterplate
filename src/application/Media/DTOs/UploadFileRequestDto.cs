using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Api.Application.Media.DTOs;

public class UploadFileRequestDto
{
    [Required]
    public IFormFile File { get; set; } = default!;
    public string? Folder { get; set; }
}

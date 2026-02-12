using System.ComponentModel.DataAnnotations;

namespace Api.Application.Media.DTOs;

public class ResizeImageRequestDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Width { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Height { get; set; }

    [StringLength(500)]
    public string? OutputFolder { get; set; }
}

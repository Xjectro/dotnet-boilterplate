using System.ComponentModel.DataAnnotations;

namespace Source.Features.Clients.DTOs;

// Example Client Request DTO
public class ClientRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

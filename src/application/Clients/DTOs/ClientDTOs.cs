using System.ComponentModel.DataAnnotations;

namespace Api.Application.Clients.DTOs;

// Example Client Request DTO
public class ClientRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Source.Features.Mail.DTOs;

public class SendMailRequest
{
    [Required(ErrorMessage = "Recipients are required")]
    [MinLength(1, ErrorMessage = "At least one recipient is required")]
    public required string[] To { get; set; }
    
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Subject must be between 1 and 500 characters")]
    public required string Subject { get; set; }
    
    [Required(ErrorMessage = "Body is required")]
    [MinLength(1, ErrorMessage = "Body cannot be empty")]
    public required string Body { get; set; }
    
    public bool IsHtml { get; set; } = true;
}

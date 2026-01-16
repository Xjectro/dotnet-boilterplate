namespace Source.DTOs;

public class SendMailRequest
{
    public required string[] To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsHtml { get; set; } = true;
}

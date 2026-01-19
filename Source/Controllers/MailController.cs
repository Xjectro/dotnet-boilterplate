using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Source.DTOs;
using Source.Services.MailService;

namespace Source.Controllers;

[ApiController]
[Route("mail")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost("send")]
    [EnableRateLimiting("strict")]
    public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
    {
        await _mailService.QueueEmailAsync(
            request.To,
            request.Subject,
            request.Body,
            request.IsHtml
        );

        return Ok(new { message = "Email successfully queued", success = true });
    }
}

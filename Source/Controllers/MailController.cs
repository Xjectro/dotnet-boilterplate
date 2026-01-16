using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Source.DTOs;
using Source.Services.MailService;

namespace Source.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;
    private readonly ILogger<MailController> _logger;

    public MailController(IMailService mailService, ILogger<MailController> logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    [HttpPost("send")]
    [EnableRateLimiting("strict")]
    public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
    {
        try
        {
            await _mailService.QueueEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                request.IsHtml
            );

            return Ok(new { message = "Email successfully queued", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while queueing email");
            return StatusCode(500, new { message = "Error occurred while queueing email", success = false });
        }
    }
}

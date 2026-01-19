using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using Source.Features.Mail.DTOs;
using Source.Services.MailService;

namespace Source.Features.Mail.Controllers;

/// <summary>
/// Email service endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/mail")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    /// <summary>
    /// Sends an email by queueing it to RabbitMQ
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await _mailService.QueueEmailAsync(request.To, request.Subject, request.Body, request.IsHtml);
        return Ok();
    }
}

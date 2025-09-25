using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Api.Attributes;

namespace Api.Controllers;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    [HttpGet]
    [RequireMember]
    public IActionResult Get()
    {
        var member = HttpContext.Items["Member"] as Member;

        return Ok(new
        {
            success = true,
            id = member?.Id,
            username = member?.Username,
            email = member?.Email
        });
    }
}

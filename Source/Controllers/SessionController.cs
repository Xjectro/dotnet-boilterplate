using Microsoft.AspNetCore.Mvc;

using Source.Models;
using Source.Attributes;

namespace Source.Controllers;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    [HttpGet]
    [RequireMember]
    public IActionResult Get()
    {
        Member member = (Member)HttpContext.Items["Member"]!;

        return Ok(new { success = true, data = member });
    }
}

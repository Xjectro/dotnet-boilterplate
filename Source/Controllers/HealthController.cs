using Microsoft.AspNetCore.Mvc;

namespace Source.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {

        return Ok(new { success = true, message = "Ok" });
    }
}

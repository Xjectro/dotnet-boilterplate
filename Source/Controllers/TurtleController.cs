using Microsoft.AspNetCore.Mvc;

namespace Source.Controllers;

[ApiController]
[Route("turtle")]
public class TurtleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {

        return Ok(new { success = true, });
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presentation.Api.Controllers.Clients;

/// <summary>
/// Health check endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns API health status
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult Get()
    {
        return Ok("Healthy");
    }
}

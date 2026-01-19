using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Source.Models;
using Source.Features.Clients.Repositories;

namespace Source.Features.Clients.Controllers;

/// <summary>
/// Client management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/client")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository _clientRepository;

    public ClientController(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    /// <summary>
    /// Get all clients
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientModel>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientRepository.GetAllAsync();
        return Ok(clients);
    }

    /// <summary>
    /// Get client by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientModel), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            return NotFound();
        return Ok(client);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(void), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] ClientModel client)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        await _clientRepository.InsertAsync(client);
        return StatusCode(201);
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientModel client)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var existing = await _clientRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        client.Id = id;
        await _clientRepository.UpdateAsync(client);
        return NoContent();
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _clientRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        await _clientRepository.DeleteAsync(id);
        return NoContent();
    }
}

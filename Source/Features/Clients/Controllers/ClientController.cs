using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Source.Models;
using Source.Features.Clients.Repositories;
using Source.Common;

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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClientModel>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientRepository.GetAllAsync();
        var response = new ApiResponse<IEnumerable<ClientModel>>(clients, "Clients listed successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get client by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClientModel>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            return NotFound(new ApiResponse<string>("Client not found", (List<string>?)null, false));
        var response = new ApiResponse<ClientModel>(client, "Client found");
        return Ok(response);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClientModel>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    public async Task<IActionResult> Create([FromBody] ClientModel client)
    {
        await _clientRepository.InsertAsync(client);
        var response = new ApiResponse<ClientModel>(client, "Client created successfully");
        return StatusCode(201, response);
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClientModel>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientModel client)
    {
        var existing = await _clientRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<string>("Client not found", (List<string>?)null, false));
        client.Id = id;
        await _clientRepository.UpdateAsync(client);
        var response = new ApiResponse<ClientModel>(client, "Client updated successfully");
        return Ok(response);
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _clientRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<string>("Client not found", (List<string>?)null, false));
        await _clientRepository.DeleteAsync(id);
        var response = new ApiResponse<string>("Client deleted successfully");
        return Ok(response);
    }
}

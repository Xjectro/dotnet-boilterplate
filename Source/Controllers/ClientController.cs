using Microsoft.AspNetCore.Mvc;
using Source.Models;
using Source.Repositories.ClientRepository;

namespace Source.Controllers;

[ApiController]
[Route("client")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository _clientRepository;

    public ClientController(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientRepository.GetAllAsync();
        return Ok(new { success = true, data = clients });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        
        if (client == null)
            return NotFound(new { success = false, message = "Client not found" });
        
        return Ok(new { success = true, data = client });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientModel client)
    {
        client.Id = Guid.NewGuid();
        await _clientRepository.InsertAsync(client);
        
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, new { success = true, data = client });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientModel client)
    {
        var existingClient = await _clientRepository.GetByIdAsync(id);
        
        if (existingClient == null)
            return NotFound(new { success = false, message = "Client not found" });
        
        client.Id = id;
        await _clientRepository.UpdateAsync(client);
        
        return Ok(new { success = true, data = client });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        
        if (client == null)
            return NotFound(new { success = false, message = "Client not found" });
        
        await _clientRepository.DeleteAsync(id);
        
        return Ok(new { success = true, message = "Client deleted successfully" });
    }
}

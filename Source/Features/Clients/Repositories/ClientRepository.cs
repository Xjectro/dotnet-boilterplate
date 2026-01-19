using Source.Models;
using Source.Services.CassandraService;
using Cassandra.Data.Linq;

namespace Source.Features.Clients.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ICassandraService _cassandraService;
    private readonly Table<ClientModel> _table;

    public ClientRepository(ICassandraService cassandraService)
    {
        _cassandraService = cassandraService;
        _table = _cassandraService.GetTable<ClientModel>();
    }

    public async Task<ClientModel?> GetByIdAsync(Guid id)
    {
        return await _table.FirstOrDefault(c => c.Id == id).ExecuteAsync();
    }

    public async Task<IEnumerable<ClientModel>> GetAllAsync()
    {
        return await _table.ExecuteAsync();
    }

    public async Task InsertAsync(ClientModel client)
    {
        await _table.Insert(client).ExecuteAsync();
    }

    public async Task UpdateAsync(ClientModel client)
    {
        await _table.Where(c => c.Id == client.Id)
            .Select(c => new ClientModel {
                Id = client.Id,
                Name = client.Name
            })
            .Update()
            .ExecuteAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _table.Where(c => c.Id == id).Delete().ExecuteAsync();
    }
}

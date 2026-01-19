using Source.Models;

namespace Source.Features.Clients.Repositories;

public interface IClientRepository
{
    Task<ClientModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClientModel>> GetAllAsync();
    Task InsertAsync(ClientModel client);
    Task UpdateAsync(ClientModel client);
    Task DeleteAsync(Guid id);
}

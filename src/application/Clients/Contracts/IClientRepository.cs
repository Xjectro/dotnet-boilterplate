using Api.Core.Domain;

namespace Api.Application.Clients.Contracts;

public interface IClientRepository
{
    Task<ClientModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClientModel>> GetAllAsync();
    Task InsertAsync(ClientModel client);
    Task UpdateAsync(ClientModel client);
    Task DeleteAsync(Guid id);
}

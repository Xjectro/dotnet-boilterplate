using Source.Models;
using Source.Services.CassandraService;
using Cassandra.Data.Linq;

namespace Source.Repositories.MediaRepository;

public class MediaRepository : IMediaRepository
{
    private readonly ICassandraService _cassandraService;
    private readonly Table<MediaModel> _table;

    public MediaRepository(ICassandraService cassandraService)
    {
        _cassandraService = cassandraService;
        _table = _cassandraService.GetTable<MediaModel>();
    }

    public async Task<MediaModel?> GetByIdAsync(Guid id)
    {
        return await _table
            .Where(m => m.Id == id)
            .FirstOrDefault()
            .ExecuteAsync();
    }

    public async Task<MediaModel?> GetByFileNameAsync(string fileName)
    {
        return await _table
            .Where(m => m.FileName == fileName)
            .FirstOrDefault()
            .ExecuteAsync();
    }

    public async Task<IEnumerable<MediaModel>> GetAllAsync()
    {
        return await _table.ExecuteAsync();
    }

    public async Task<IEnumerable<MediaModel>> GetByFolderAsync(string folder)
    {
        return await _table
            .Where(m => m.Folder == folder)
            .ExecuteAsync();
    }

    public async Task InsertAsync(MediaModel media)
    {
        await _table.Insert(media).ExecuteAsync();
    }

    public async Task UpdateAsync(MediaModel media)
    {
        media.UpdatedAt = DateTimeOffset.UtcNow;
        await _table.Insert(media).ExecuteAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _table
            .Where(m => m.Id == id)
            .Delete()
            .ExecuteAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var result = await _table
            .Where(m => m.Id == id)
            .Select(m => m.Id)
            .FirstOrDefault()
            .ExecuteAsync();
        
        return result != Guid.Empty;
    }
}

using Api.Application.Common.Interfaces;
using Api.Application.Media.Repositories;
using Api.Core.Domain;
using Cassandra.Data.Linq;

namespace Api.Infrastructure.Persistence.Repositories.Media;

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
        return await _table.FirstOrDefault(m => m.Id == id).ExecuteAsync();
    }

    public async Task<MediaModel?> GetByFileNameAsync(string fileName)
    {
        return await _table.FirstOrDefault(m => m.FileName == fileName).ExecuteAsync();
    }

    public async Task<IEnumerable<MediaModel>> GetAllAsync()
    {
        return await _table.ExecuteAsync();
    }

    public async Task<IEnumerable<MediaModel>> GetByFolderAsync(string folder)
    {
        return await _table.Where(m => m.Folder == folder).ExecuteAsync();
    }

    public async Task InsertAsync(MediaModel media)
    {
        await _table.Insert(media).ExecuteAsync();
    }

    public async Task UpdateAsync(MediaModel media)
    {
        await _table.Where(m => m.Id == media.Id)
            .Select(m => new MediaModel
            {
                Id = media.Id,
                FileName = media.FileName,
                OriginalName = media.OriginalName,
                ContentType = media.ContentType,
                FileSize = media.FileSize,
                Folder = media.Folder,
                Url = media.Url,
                ThumbnailUrl = media.ThumbnailUrl,
                FileExtension = media.FileExtension,
                IsImage = media.IsImage,
                Width = media.Width,
                Height = media.Height,
                UploadedAt = media.UploadedAt,
                UpdatedAt = media.UpdatedAt
            })
            .Update()
            .ExecuteAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _table.Where(m => m.Id == id).Delete().ExecuteAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var result = await _table.FirstOrDefault(m => m.Id == id).ExecuteAsync();
        return result != null;
    }
}

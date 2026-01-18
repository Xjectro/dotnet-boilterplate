using Source.Models;

namespace Source.Repositories.MediaRepository;

public interface IMediaRepository
{
    Task<MediaModel?> GetByIdAsync(Guid id);
    Task<MediaModel?> GetByFileNameAsync(string fileName);
    Task<IEnumerable<MediaModel>> GetAllAsync();
    Task<IEnumerable<MediaModel>> GetByFolderAsync(string folder);
    Task InsertAsync(MediaModel media);
    Task UpdateAsync(MediaModel media);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

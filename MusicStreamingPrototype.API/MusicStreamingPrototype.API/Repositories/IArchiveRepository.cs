
namespace MusicStreamingPrototype.API.Models
{public interface IArchiveRepository
    {
        Task<IEnumerable<int>> GetArchivedTrackIdsAsync(int userId);
        Task AddAsync(int userId, int trackId);
        Task RemoveAsync(int userId, int trackId);
    }
}
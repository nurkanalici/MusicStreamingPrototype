using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Repositories
{
    public interface ITrackRepository
    {
        Task<IEnumerable<Track>> GetAllAsync();
        Task<Track?> GetByIdAsync(int id);
        Task<Track> AddAsync(Track track);
        Task UpdateAsync(Track track);
        Task<IEnumerable<Track>> GetByPlaylistIdAsync(int playlistId);
        Task DeleteAsync(int id);

        Task<IEnumerable<Track>> SearchAsync(string query);

       
        Task<bool> UpdatePlaylistAsync(int trackId, int? playlistId);
    }
}

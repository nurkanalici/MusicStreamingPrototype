
using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Repositories
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetAllAsync();
        Task<Playlist?> GetByIdAsync(int id);
        Task<Playlist> AddAsync(Playlist playlist);
        Task UpdateAsync(Playlist playlist);
        Task<bool> ExistsAsync(int id);
        Task DeleteAsync(int id);

        
        Task<Playlist?> GetBySpotifyIdAsync(string spotifyId);

        Task AddTrackAsync(int playlistId, int trackId);

        Task RemoveTrackAsync(int playlistId, int trackId);
    }
}

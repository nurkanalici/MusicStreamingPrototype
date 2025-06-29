
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicStreamingPrototype.UI.Models;

namespace MusicStreamingPrototype.UI.Services
{
    public interface IApiClient
    {
        
        Task<bool> RegisterAsync(string username, string password);
        Task<string?> LoginAsync(string username, string password);
        void StoreTokenInSession(string token);

        
        Task<IEnumerable<Playlist>> GetPlaylistsAsync();
        Task<Playlist?> GetPlaylistByIdAsync(int playlistId);
        Task<Playlist> CreatePlaylistAsync(Playlist playlist);

        
        Task<IEnumerable<Track>> SearchTracksAsync(string query);
        Task<IEnumerable<Track>> GetTracksInPlaylistAsync(int playlistId);

        
        Task<IEnumerable<int>> GetArchivedTrackIdsAsync();
        Task<bool> AddToArchiveAsync(int trackId);
        Task<bool> RemoveFromArchiveAsync(int trackId);

        
        Task<bool> AddTrackToPlaylistAsync(int playlistId, int trackId);
        Task<bool> RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
        Task<Track?> GetTrackByIdAsync(int trackId);
    }
}

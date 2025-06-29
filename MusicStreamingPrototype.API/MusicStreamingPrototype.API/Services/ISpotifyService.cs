using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Services
{
    
    public interface ISpotifyService
    {
        
        Task<string> GetAccessTokenAsync();

        
        Task<object> GetPlaylistAsync(string playlistId);

       
        Task<object> GetRecommendationsAsync(string seedTrackId);
        
        Task<object> GetTrackAsync(string trackId);

        
       
        Task<IEnumerable<Playlist>> GetSomePlaylistsAsync(int take = 10);
    
    }
}

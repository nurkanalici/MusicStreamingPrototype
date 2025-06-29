using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using MusicStreamingPrototype.API.Services;
using MusicStreamingPrototype.API.Repositories;
using MusicStreamingPrototype.API.Models;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly ISpotifyService _spotify;
        private readonly IPlaylistRepository _playlistRepo;
        private readonly ITrackRepository _trackRepo;

        public SpotifyController(
            ISpotifyService spotify,
            IPlaylistRepository playlistRepo,
            ITrackRepository trackRepo)
        {
            _spotify = spotify;
            _playlistRepo = playlistRepo;
            _trackRepo = trackRepo;
        }

        [HttpGet("track/{id}")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetTrack(string id)
        {
            try
            {
                var dataObj = await _spotify.GetTrackAsync(id);
                if (dataObj is JsonElement elem)
                {
                    
                    var playlists = (await _playlistRepo.GetAllAsync()).ToList();
                    var defaultPlaylist = playlists.FirstOrDefault()
                        ?? await _playlistRepo.AddAsync(new Playlist
                        {
                            SpotifyId = "default",
                            Name = "Default Playlist",
                            Description = "Auto-created default playlist"
                        });

                    
                    var track = new Track
                    {
                        SpotifyId = elem.GetProperty("id").GetString()!,
                        Title = elem.GetProperty("name").GetString()!,
                        Artist = elem.GetProperty("artists")[0].GetProperty("name").GetString()!,
                        PlaylistId = defaultPlaylist.Id
                    };
                    await _trackRepo.AddAsync(track);

                    return Content(elem.GetRawText(), "application/json");
                }
                return NotFound();
            }
            catch (ApplicationException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
        }

        [HttpGet("playlist/{id}")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetPlaylist(string id)
        {
            try
            {
                var dataObj = await _spotify.GetPlaylistAsync(id);
                if (dataObj is JsonElement elem)
                {
                    
                    var playlists = (await _playlistRepo.GetAllAsync()).ToList();
                    var created = playlists.FirstOrDefault(p => p.SpotifyId == id)
                        ?? await _playlistRepo.AddAsync(new Playlist
                        {
                            SpotifyId = elem.GetProperty("id").GetString()!,
                            Name = elem.GetProperty("name").GetString()!,
                            Description = elem.GetProperty("description").GetString() ?? string.Empty
                        });

                    
                    if (elem.TryGetProperty("tracks", out var tracksNode) &&
                        tracksNode.TryGetProperty("items", out var items))
                    {
                        var existingTracks = (await _trackRepo.GetAllAsync()).ToList();
                        foreach (var item in items.EnumerateArray())
                        {
                            var trackElem = item.GetProperty("track");
                            var spotifyId = trackElem.GetProperty("id").GetString()!;
                            
                            if (existingTracks.Any(t => t.SpotifyId == spotifyId && t.PlaylistId == created.Id))
                                continue;

                            var track = new Track
                            {
                                SpotifyId = spotifyId,
                                Title = trackElem.GetProperty("name").GetString()!,
                                Artist = trackElem.GetProperty("artists")[0].GetProperty("name").GetString()!,
                                PlaylistId = created.Id
                            };
                            await _trackRepo.AddAsync(track);
                        }
                    }

                    return Content(elem.GetRawText(), "application/json");
                }
                return NotFound();
            }
            catch (ApplicationException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.API.Models;
using MusicStreamingPrototype.API.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/playlists")]
    [Authorize]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistRepository _playlistRepo;
        private readonly ITrackRepository _trackRepo;

        public PlaylistsController(
            IPlaylistRepository playlistRepo,
            ITrackRepository trackRepo)
        {
            _playlistRepo = playlistRepo;
            _trackRepo = trackRepo;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetAll()
        {
            var playlists = await _playlistRepo.GetAllAsync();
            return Ok(playlists);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> Get(int id)
        {
            var playlist = await _playlistRepo.GetByIdAsync(id);
            if (playlist == null)
                return NotFound();
            return Ok(playlist);
        }

        
        [HttpGet("{id}/tracks")]
        public async Task<ActionResult<IEnumerable<Track>>> GetTracks(int id)
        {
            if (!await _playlistRepo.ExistsAsync(id))
                return NotFound();

            var tracks = await _trackRepo.GetByPlaylistIdAsync(id);
            return Ok(tracks);
        }

        
        [HttpPost]
        public async Task<ActionResult<Playlist>> Create(Playlist playlist)
        {
            var created = await _playlistRepo.AddAsync(playlist);
            return CreatedAtAction(nameof(Get),
                new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = created.Id },
                created);
        }

        
        [HttpPost("{id}/tracks")]
        public async Task<IActionResult> AddTrack(int id, [FromBody] AddTrackRequest req)
        {
            if (!await _playlistRepo.ExistsAsync(id))
                return NotFound();

            var ok = await _trackRepo.UpdatePlaylistAsync(req.TrackId, id);
            if (!ok) return NotFound();

            return NoContent();
        }

        
        [HttpDelete("{id}/tracks/{trackId}")]
        public async Task<IActionResult> RemoveTrack(int id, int trackId)
        {
            if (!await _playlistRepo.ExistsAsync(id))
                return NotFound();

            
            var ok = await _trackRepo.UpdatePlaylistAsync(trackId, null);
            if (!ok) return NotFound();

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _playlistRepo.ExistsAsync(id))
                return NotFound();

            await _playlistRepo.DeleteAsync(id);
            return NoContent();
        }

    }

    public record AddTrackRequest(int TrackId);
}

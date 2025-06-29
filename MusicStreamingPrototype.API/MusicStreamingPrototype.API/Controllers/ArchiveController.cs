
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.API.Models;
using System.Security.Claims;
using System.Text.Json;

namespace MusicStreamingPrototype.API.Models

{[ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveRepository _repo;
        public ArchiveController(IArchiveRepository repo) => _repo = repo;

        int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]                      
        public async Task<IActionResult> Get() =>
            Ok(await _repo.GetArchivedTrackIdsAsync(CurrentUserId));

        [HttpPost]                     
        public async Task<IActionResult> Add([FromBody] JsonElement d)
        {
            var trackId = d.GetProperty("trackId").GetInt32();
            await _repo.AddAsync(CurrentUserId, trackId);
            return NoContent();
        }

        [HttpDelete("{trackId}")]      
        public async Task<IActionResult> Remove(int trackId)
        {
            await _repo.RemoveAsync(CurrentUserId, trackId);
            return NoContent();
        }
    }
}
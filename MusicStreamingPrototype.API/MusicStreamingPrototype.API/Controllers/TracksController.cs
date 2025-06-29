using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using MusicStreamingPrototype.API.Models;
using MusicStreamingPrototype.API.Repositories;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]  
    public class TracksController : ControllerBase
    {
        private readonly ITrackRepository _repo;
        public TracksController(ITrackRepository repo) => _repo = repo;

        
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAsync());

        
        [HttpGet("{id}")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _repo.GetByIdAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var results = await _repo.SearchAsync(query);
            return Ok(results);
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(Track track)
        {
            var created = await _repo.AddAsync(track);
            return CreatedAtAction(nameof(Get),
                new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = created.Id },
                created);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Track track)
        {
            if (id != track.Id) return BadRequest();
            await _repo.UpdateAsync(track);
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}

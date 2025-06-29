using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.API.Models;
using MusicStreamingPrototype.API.Repositories;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        public UsersController(IUserRepository repo) => _repo = repo;

        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.GetAllAsync());

        
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User u)
        {
            if (id != u.Id) return BadRequest();
            await _repo.UpdateAsync(u);
            return NoContent();
        }

        
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _repo.GetByUsernameAsync(req.Username);
            if (existing != null)
                return Conflict(new { error = "Kullanıcı zaten kayıtlı." });

            
            var user = new User
            {
                Username = req.Username,
                PasswordHash = req.Password,
                Role = "User"
            };
            var created = await _repo.AddAsync(user);
            return CreatedAtAction(null, new { id = created.Id }, null);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Models
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _ctx;
        public UserRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<User> AddAsync(User user)
        {
            await _ctx.Users.AddAsync(user);
            await _ctx.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _ctx.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _ctx.Users.AsNoTracking().ToListAsync();

        public async Task UpdateAsync(User user)
        {
            _ctx.Users.Update(user);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var u = await _ctx.Users.FindAsync(id);
            if (u == null) return;
            _ctx.Users.Remove(u);
            await _ctx.SaveChangesAsync();
        }
    }
}
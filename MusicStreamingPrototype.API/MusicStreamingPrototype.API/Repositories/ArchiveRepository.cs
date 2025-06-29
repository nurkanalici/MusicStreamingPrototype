
using Microsoft.EntityFrameworkCore;       
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;

namespace MusicStreamingPrototype.API.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly ApplicationDbContext _ctx;
        public ArchiveRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IEnumerable<int>> GetArchivedTrackIdsAsync(int userId)
        {
            
            return await _ctx.Archives
                             .Where(a => a.UserId == userId)
                             .Select(a => a.TrackId)
                             .ToListAsync();
        }

        public async Task AddAsync(int userId, int trackId)
        {
            bool exists = await _ctx.Archives
                                    .AnyAsync(a => a.UserId == userId && a.TrackId == trackId);

            if (!exists)
            {
                await _ctx.Archives.AddAsync(new UserTrackArchive
                {
                    UserId = userId,
                    TrackId = trackId
                });
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(int userId, int trackId)
        {
            var ent = await _ctx.Archives
                                .FirstOrDefaultAsync(a => a.UserId == userId && a.TrackId == trackId);

            if (ent is not null)
            {
                _ctx.Archives.Remove(ent);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}

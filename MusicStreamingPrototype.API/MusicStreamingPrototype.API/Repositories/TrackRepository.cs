
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        private readonly ApplicationDbContext _ctx;
        public TrackRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IEnumerable<Track>> GetAllAsync() =>
            await _ctx.Tracks
                      .AsNoTracking()
                      .Include(t => t.Playlist)
                      .ToListAsync();

        public async Task<Track?> GetByIdAsync(int id) =>
            await _ctx.Tracks
                      .AsNoTracking()
                      .Include(t => t.Playlist)
                      .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Track> AddAsync(Track track)
        {
            await _ctx.Tracks.AddAsync(track);
            await _ctx.SaveChangesAsync();
            return track;
        }

        public async Task UpdateAsync(Track track)
        {
            _ctx.Tracks.Update(track);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var track = await _ctx.Tracks.FindAsync(id);
            if (track == null) return;
            _ctx.Tracks.Remove(track);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<Track>> GetByPlaylistIdAsync(int playlistId)
        {
            return await _ctx.Tracks
                .Where(t => t.PlaylistId == playlistId)
                .ToListAsync();
        }

        /// <summary>
        /// Güncelleme: Bir parçayı belirli bir çalma listesine atar veya null ile listeden çıkarır.
        /// </summary>
        /// <param name="trackId">Şarkı kimliği</param>
        /// <param name="playlistId">Ataılacak playlistId veya null</param>
        public async Task<bool> UpdatePlaylistAsync(int trackId, int? playlistId)
        {
            var track = await _ctx.Tracks.FindAsync(trackId);
            if (track == null) return false;

            track.PlaylistId = playlistId;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Track>> SearchAsync(string query) =>
            await _ctx.Tracks
                      .AsNoTracking()
                      .Include(t => t.Playlist)
                      .Where(t =>
                          EF.Functions.Like(t.Title, $"%{query}%") ||
                          EF.Functions.Like(t.Artist, $"%{query}%")
                      )
                      .ToListAsync();
    }
}


using Microsoft.EntityFrameworkCore;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ApplicationDbContext _ctx;
        public PlaylistRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IEnumerable<Playlist>> GetAllAsync() =>
            await _ctx.Playlists.Include(p => p.Tracks).ToListAsync();

        public async Task<Playlist?> GetByIdAsync(int id) =>
            await _ctx.Playlists.Include(p => p.Tracks)
                                 .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Playlist> AddAsync(Playlist playlist)
        {
            _ctx.Playlists.Add(playlist);
            await _ctx.SaveChangesAsync();
            return playlist;
        }

        public async Task UpdateAsync(Playlist playlist)
        {
            _ctx.Playlists.Update(playlist);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pl = await _ctx.Playlists.FindAsync(id);
            if (pl != null) { _ctx.Playlists.Remove(pl); await _ctx.SaveChangesAsync(); }
        }

        public async Task<Playlist?> GetBySpotifyIdAsync(string spotifyId) =>
            await _ctx.Playlists.FirstOrDefaultAsync(p => p.SpotifyId == spotifyId);

        public async Task AddTrackAsync(int playlistId, int trackId)
        {
            var track = await _ctx.Tracks.FindAsync(trackId);
            if (track == null) throw new KeyNotFoundException("Track bulunamadı");
            track.PlaylistId = playlistId;
            await _ctx.SaveChangesAsync();
        }

        public async Task RemoveTrackAsync(int playlistId, int trackId)
        {
            var track = await _ctx.Tracks.FindAsync(trackId);
            if (track != null && track.PlaylistId == playlistId)
            {
                _ctx.Tracks.Remove(track);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _ctx.Playlists.AnyAsync(p => p.Id == id);
        }
    }
}

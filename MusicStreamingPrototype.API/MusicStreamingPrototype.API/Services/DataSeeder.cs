using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MusicStreamingPrototype.API.Repositories;
using MusicStreamingPrototype.API.Services;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var spotify = scope.ServiceProvider.GetRequiredService<ISpotifyService>();
            var plRepo = scope.ServiceProvider.GetRequiredService<IPlaylistRepository>();
            var trRepo = scope.ServiceProvider.GetRequiredService<ITrackRepository>();

            
            var initialPlaylistIds = new[] { "37i9dQZF1DXcBWIGoYBM5M", "37i9dQZF1DJAZH2giILv5T" };

            foreach (var spId in initialPlaylistIds)
            {
                if (await plRepo.GetBySpotifyIdAsync(spId) != null)
                    continue;

                var elem = (JsonElement)await spotify.GetPlaylistAsync(spId);
                var playlist = new Models.Playlist
                {
                    SpotifyId = elem.GetProperty("id").GetString()!,
                    Name = elem.GetProperty("name").GetString()!,
                    Description = elem.GetProperty("description").GetString() ?? ""
                };
                await plRepo.AddAsync(playlist);

                if (elem.TryGetProperty("tracks", out var tracksNode)
                    && tracksNode.TryGetProperty("items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var t = item.GetProperty("track");
                        var track = new Models.Track
                        {
                            SpotifyId = t.GetProperty("id").GetString()!,
                            Title = t.GetProperty("name").GetString()!,
                            Artist = t.GetProperty("artists")[0].GetProperty("name").GetString()!,
                            PlaylistId = playlist.Id
                        };
                        await trRepo.AddAsync(track);
                    }
                }
            }
        }
    }
}

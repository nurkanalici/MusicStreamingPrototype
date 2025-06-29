
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicStreamingPrototype.API.Models;
using MusicStreamingPrototype.API.Services;
using BCrypt.Net;                    

namespace MusicStreamingPrototype.API.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(this ApplicationDbContext ctx,
                                           ISpotifyService spotify)
        {
           
            if (!await ctx.Playlists.AnyAsync())
            {
                var playlists = (await spotify.GetSomePlaylistsAsync(10)).ToList();

                
                if (!playlists.Any())
                {
                    playlists = new List<Playlist>
                    {
                        new Playlist
                        {
                            Name        = "Demo Playlist",
                            SpotifyId   = "demo",
                            Description = "Seed fallback playlist",
                            CoverUrl    = null
                        }
                    };
                }

                
                foreach (var pl in playlists.Where(p => string.IsNullOrWhiteSpace(p.Description)))
                    pl.Description = "Imported from Spotify";

                ctx.Playlists.AddRange(playlists);
                await ctx.SaveChangesAsync();
            }

            
            if (!await ctx.Users.AnyAsync())
            {
                var users = new[]
                {
                    new User
                    {
                        Username     = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role         = "Admin"
                    },
                    new User
                    {
                        Username     = "nurkan",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                        Role         = "User"
                    }
                };

                ctx.Users.AddRange(users);
                await ctx.SaveChangesAsync();
            }
        }
    }
}

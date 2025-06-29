using Microsoft.EntityFrameworkCore;
using MusicStreamingPrototype.API.Models;

namespace MusicStreamingPrototype.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        
        public DbSet<User> Users => Set<User>();
        public DbSet<Playlist> Playlists => Set<Playlist>();
        public DbSet<Track> Tracks => Set<Track>();

        
        public DbSet<UserTrackArchive> Archives => Set<UserTrackArchive>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Track>()
                .HasOne(t => t.Playlist)
                .WithMany(p => p.Tracks)
                .HasForeignKey(t => t.PlaylistId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            
            modelBuilder.Entity<UserTrackArchive>(entity =>
            {
                
                entity.ToTable("UserTrackArchive");

                
                entity.HasKey(a => new { a.UserId, a.TrackId });

                
                entity.HasOne(a => a.User)
                    .WithMany(u => u.UserTrackArchives)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasOne(a => a.Track)
                    .WithMany()
                    .HasForeignKey(a => a.TrackId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

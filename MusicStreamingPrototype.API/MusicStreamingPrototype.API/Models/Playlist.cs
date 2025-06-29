using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicStreamingPrototype.API.Models
{
    public class Playlist
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SpotifyId { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string Description { get; set; } = "";

        public string? CoverUrl { get; set; }

        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}

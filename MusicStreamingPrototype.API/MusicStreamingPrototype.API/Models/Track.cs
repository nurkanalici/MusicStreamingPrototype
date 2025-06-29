using System.ComponentModel.DataAnnotations;

namespace MusicStreamingPrototype.API.Models
{
    public class Track
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SpotifyId { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = default!;

        
        public int? PlaylistId { get; set; }

        
        public Playlist? Playlist { get; set; }
    }
}

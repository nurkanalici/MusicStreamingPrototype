using System.ComponentModel.DataAnnotations;

namespace MusicStreamingPrototype.UI.Models
{
    public class PlaylistViewModel
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}

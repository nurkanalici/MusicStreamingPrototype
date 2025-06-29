
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamingPrototype.API.Models
{
    [Table("UserTrackArchive")]
    public class UserTrackArchive
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TrackId { get; set; }
        public Track Track { get; set; } = null!;
    }
}


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicStreamingPrototype.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [Required]
        public string Role { get; set; } = "User";   

        public ICollection<UserTrackArchive> UserTrackArchives { get; set; } = new List<UserTrackArchive>();
    }
}

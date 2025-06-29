using System.Collections.Generic;
using System.Linq;
using MusicStreamingPrototype.UI.Models;   

namespace MusicStreamingPrototype.UI.Models
{
    public class UserHomeViewModel
    {
        public string Query { get; set; } = "";

        
        public IEnumerable<Track> SearchResults { get; set; } = Enumerable.Empty<Track>();
        public IEnumerable<int> ArchivedTrackIds { get; set; } = Enumerable.Empty<int>();

        public IEnumerable<Playlist> Playlists { get; set; } = Enumerable.Empty<Playlist>();
    }
}

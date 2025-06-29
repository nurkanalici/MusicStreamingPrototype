namespace MusicStreamingPrototype.UI.Models
{
    public class PlaylistDetailsViewModel
    {
        public Playlist Playlist { get; set; } = null!;
        public List<Track> Tracks { get; set; } = new();
        public HashSet<int> ArchivedTrackIds { get; set; } = new();
        public IEnumerable<Track> ArchivedTracks { get; set; } = Enumerable.Empty<Track>();
    }
}

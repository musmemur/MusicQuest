namespace Backend.Entities;

public class PlaylistTrack
{
    public Guid PlaylistId { get; set; }
    public Playlist Playlist { get; set; }
    public Guid TrackId { get; set; }
    public Track Track { get; set; }
}
namespace Backend.Domain.Entities;

public class Playlist(Guid userId, string title)
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = userId;
    public string Title { get; set; } = title;
    public Guid? GameSessionId { get; set; }
    public List<PlaylistTrack> PlaylistTracks { get; set; } = [];
}
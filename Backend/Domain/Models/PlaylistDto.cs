namespace Backend.Domain.Models;

public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<TrackDto> Tracks { get; set; }
}
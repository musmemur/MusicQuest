namespace Backend.Application.Models;

public class TrackDto
{
    public Guid Id { get; set; }
    public long DeezerTrackId { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string PreviewUrl { get; set; }
    public string CoverUrl { get; set; }
}
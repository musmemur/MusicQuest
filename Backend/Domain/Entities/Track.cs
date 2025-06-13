using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.Entities;

public class Track
{
    [Key]
    public Guid Id { get; set; } 
    public long DeezerTrackId { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string PreviewUrl { get; set; }
    public string CoverUrl { get; set; }
    
    public List<PlaylistTrack> PlaylistTracks { get; set; } = new();
}
namespace Backend.Dto;

public class UserPlaylistsDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string? UserPhoto { get; set; }
    public List<PlaylistDto> Playlists { get; set; }
}

public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<PlaylistTrackDto> PlaylistTracks { get; set; }
}

public class PlaylistTrackDto
{
    public TrackDto Track { get; set; }
}

public class TrackDto
{
    public Guid Id { get; set; }
    public long DeezerTrackId { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string PreviewUrl { get; set; }
    public string CoverUrl { get; set; }
}
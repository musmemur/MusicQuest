namespace Backend.Domain.Models;

public class UserPlaylistsDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string? UserPhoto { get; set; }
    public List<PlaylistDto> Playlists { get; set; }
}
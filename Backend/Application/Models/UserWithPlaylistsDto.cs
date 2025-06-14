namespace Backend.Application.Models;

public class UserWithPlaylistsDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string? UserPhoto { get; set; }
    public List<PlaylistDto> Playlists { get; set; } = new();
}
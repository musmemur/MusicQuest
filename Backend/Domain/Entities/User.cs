namespace Backend.Domain.Entities;

public class User(string username, string password, string? userPhoto)
{
    public Guid Id { get; set; }
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string? UserPhoto { get; set; } = userPhoto;

    public List<Playlist> Playlists { get; set; } = [];
}

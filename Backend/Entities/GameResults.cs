namespace Backend.Entities;

public class GameResult
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    public GameSession GameSession { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
    public string Genre { get; set; }
    public Guid WinnerId { get; set; }
    public User Winner { get; set; }
    public Guid? PlaylistId { get; set; }
    public Playlist Playlist { get; set; }
    public List<PlayerScore> PlayerScores { get; set; } = new();
}
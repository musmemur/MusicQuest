namespace Backend.Application.Models;

public class GameResultDto
{
    public Guid GameId { get; set; }
    public Guid RoomId { get; set; }
    public string Genre { get; set; }
    public List<string> Winners { get; set; }
    public List<string> WinnerNames { get; set; }
    public Dictionary<string, PlayerScoreDto> Scores { get; set; }
}
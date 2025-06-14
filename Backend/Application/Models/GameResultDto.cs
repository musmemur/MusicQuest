namespace Backend.Application.Models;

public class GameResultDto
{
    public Guid GameId { get; set; }
    public Guid RoomId { get; set; }
    public string Genre { get; set; }
    public Guid WinnerId { get; set; }
    public string WinnerName { get; set; }
    public Dictionary<string, PlayerScoreDto> Scores { get; set; }
}
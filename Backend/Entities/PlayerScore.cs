namespace Backend.Entities;

public class PlayerScore
{
    public Guid Id { get; set; }
    public Guid GameResultId { get; set; }
    public GameResult GameResult { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public int Score { get; set; }
}
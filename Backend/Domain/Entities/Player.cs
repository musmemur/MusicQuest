namespace Backend.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public int Score { get; set; } 
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
}
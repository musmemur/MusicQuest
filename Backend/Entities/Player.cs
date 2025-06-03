namespace Backend.Entities;

public class Player
{
    public Guid Id { get; set; }
    public int Score { get; set; } = 0; // Очки игрока
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Связь с User (игрок — это пользователь)
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    // Связь с Room (в какой комнате находится)
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
}
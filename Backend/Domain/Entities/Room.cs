using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Новая комната";
    public DeezerGenre Genre { get; set; }
    public bool IsActive { get; set; } = true; 
    
    public int QuestionsCount { get; set; }
    
    public User HostUser { get; set; }
    public Guid HostUserId { get; set; }
    
    public List<Player> Players { get; set; } = new();
}
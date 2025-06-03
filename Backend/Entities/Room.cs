namespace Backend.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Новая комната";
    public string Genre { get; set; } // Например: "Рок", "Поп"
    public bool IsActive { get; set; } = true; // Закрыта/открыта
    
    public int QuestionsCount { get; set; }
    
    // Владелец комнаты (связь с User)
    public User HostUser { get; set; }
    public Guid HostUserId { get; set; }
    
    // Игроки в комнате (связь с Player)
    public List<Player> Players { get; set; } = new();
}
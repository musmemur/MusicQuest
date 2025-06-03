using Backend.Modals;

namespace Backend.Entities;

public class GameSession
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Waiting";
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    public int QuestionsCount { get; set; }
    
    public int CurrentQuestionIndex { get; set; }
    
    // Связь с Room (к какой комнате относится)
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
    
    // Список вопросов в сессии
    public List<QuizQuestion> Questions { get; set; } = new();
}
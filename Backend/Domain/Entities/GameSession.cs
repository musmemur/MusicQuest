namespace Backend.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Waiting";
    public int QuestionsCount { get; set; }
    public int CurrentQuestionIndex { get; set; }
    
    public Guid RoomId { get; set; }
    public Room Room { get; set; }
    
    public List<QuizQuestion> Questions { get; set; } = new();
}
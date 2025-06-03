namespace Backend.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; }
    public string CorrectAnswer { get; set; }
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string QuestionType { get; set; } // "artist" или "track"
    public string PreviewUrl { get; set; }
    public string CoverUrl { get; set; }
    
    // Связь с GameSession
    public Guid GameSessionId { get; set; }
    public GameSession GameSession { get; set; }
}
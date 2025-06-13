using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IQuizQuestionRepository
{
    Task RemoveRangeAsync(IEnumerable<QuizQuestion> quizQuestions);
}
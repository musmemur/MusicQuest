using Backend.Entities;

namespace Backend.Repositories;

public interface IQuizQuestionRepository
{
    Task RemoveRangeAsync(IEnumerable<QuizQuestion> quizQuestions);
}
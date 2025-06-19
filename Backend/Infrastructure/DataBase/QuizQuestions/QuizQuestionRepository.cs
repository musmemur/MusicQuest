using Backend.Domain.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Infrastructure.DataBase.QuizQuestions;

public class QuizQuestionRepository(AppDbContext dbContext) : IQuizQuestionRepository
{
    public async Task RemoveRangeAsync(IEnumerable<QuizQuestion> quizQuestions)
    {
        dbContext.QuizQuestions.RemoveRange(quizQuestions);
        await dbContext.SaveChangesAsync();
    }
}
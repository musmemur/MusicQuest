using Backend.DataBase;
using Backend.Entities;

namespace Backend.Repositories;

public class QuizQuestionRepository(AppDbContext dbContext) : IQuizQuestionRepository
{
    public async Task RemoveRangeAsync(IEnumerable<QuizQuestion> quizQuestions)
    {
        dbContext.QuizQuestions.RemoveRange(quizQuestions);
        await dbContext.SaveChangesAsync();
    }
}
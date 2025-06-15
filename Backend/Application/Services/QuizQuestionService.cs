using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class QuizQuestionService(IDeezerApiClient deezerClient, 
    ILogger<IQuizQuestionService> logger) : IQuizQuestionService
{
    public async Task<List<QuizQuestion>> GenerateQuestionsAsync(DeezerGenre genre, int count)
    {
        var questions = new List<QuizQuestion>();
        for (var i = 0; i < count; i++)
        {
            var questionType = (i % 2 == 0) ? "artist" : "track";
            try
            {
                var question = await deezerClient.GenerateQuizQuestionAsync(genre, questionType);
                questions.Add(question);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating question {Index}", i);
                throw;
            }
        }
        return questions;
    }
}
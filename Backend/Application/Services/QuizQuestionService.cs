using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Backend.Api.Services;

public class QuizQuestionService(DeezerApiClient deezerClient, ILogger<QuizQuestionService> logger)
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
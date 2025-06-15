using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Abstractions;

public interface IQuizQuestionService
{
    Task<List<QuizQuestion>> GenerateQuestionsAsync(DeezerGenre genre, int count);
}
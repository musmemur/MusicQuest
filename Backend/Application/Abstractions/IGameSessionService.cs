using Backend.Application.Models;
using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IGameSessionService
{
    Task<string> StartNewSessionAsync(string roomId, List<QuizQuestion> questions);
    Task<QuizQuestion?> GetNextQuestionAsync(string gameSessionId);

    Task<int> ProcessAnswerAsync(string userId, string gameSessionId, int answerIndex, int questionIndex,
        int remainingTime);

    Task EndSessionAsync(string gameSessionId);
    Task<GameResultDto> PrepareGameResultsAsync(string gameSessionId);
}
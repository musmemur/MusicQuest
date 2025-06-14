using Backend.Application.Models;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;

namespace Backend.Application.Services;

public class GameSessionService(
    IGameSessionRepository gameSessionRepository,
    IPlayerRepository playerRepository,
    IRoomRepository roomRepository,
    ILogger<GameSessionService> logger, DeezerApiClient deezerApiClient)
{
    public async Task<string> StartNewSessionAsync(string roomId, List<QuizQuestion> questions)
    {
        var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));
        if (room == null)
        {
            logger.LogWarning("Room {RoomId} not found", roomId);
            throw new ArgumentException("Room not found");
        }

        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = "InProgress",
            RoomId = room.Id,
            Questions = questions,
            QuestionsCount = room.QuestionsCount,
            CurrentQuestionIndex = 0
        };

        await gameSessionRepository.AddAsync(gameSession);
        logger.LogInformation("Started new game session {SessionId}", gameSession.Id);

        return gameSession.Id.ToString();
    }

    public async Task<QuizQuestion?> GetNextQuestionAsync(string gameSessionId)
    {
        var gameSession = await gameSessionRepository.GetWithQuestionsAsync(Guid.Parse(gameSessionId));
        if (gameSession == null || gameSession.CurrentQuestionIndex >= gameSession.Questions.Count)
        {
            return null;
        }

        var question = gameSession.Questions[gameSession.CurrentQuestionIndex];
        gameSession.CurrentQuestionIndex++;
        await gameSessionRepository.UpdateAsync(gameSession);

        return question;
    }

    public async Task<int> ProcessAnswerAsync(string userId, string gameSessionId, int answerIndex, int questionIndex, int remainingTime)
    {
        var gameSession = await gameSessionRepository.GetWithQuestionsAsync(Guid.Parse(gameSessionId));
        if (gameSession == null || questionIndex >= gameSession.Questions.Count)
        {
            throw new ArgumentException("Invalid game session or question index");
        }

        var player = await playerRepository.GetPlayerInRoomAsync(Guid.Parse(userId), gameSession.RoomId);
        if (player == null)
        {
            throw new ArgumentException("Player not found");
        }

        var question = gameSession.Questions[questionIndex];
        if (answerIndex == question.CorrectIndex)
        {
            player.Score += remainingTime;
            await playerRepository.UpdateAsync(player);
        }

        return player.Score;
    }

    public async Task EndSessionAsync(string gameSessionId)
    {
        await gameSessionRepository.EndGameSessionAsync(Guid.Parse(gameSessionId));
        logger.LogInformation("Ended game session {SessionId}", gameSessionId);
    }
    
    public async Task<GameResultDto> PrepareGameResultsAsync(string gameSessionId)
    {
        var gameSession = await gameSessionRepository.GetWithRoomPlayersAndQuestionsAsync(Guid.Parse(gameSessionId));
        if (gameSession == null)
        {
            throw new ArgumentException("Game session not found");
        }

        var winner = gameSession.Room.Players.OrderByDescending(p => p.Score).First();
        var scores = gameSession.Room.Players.ToDictionary(
            p => p.UserId.ToString(),
            p => new PlayerScoreDto
            {
                Username = p.User.Username,
                UserPhoto = p.User.UserPhoto,
                Score = p.Score
            });

        return new GameResultDto
        {
            GameId = gameSession.Id,
            RoomId = gameSession.RoomId,
            Genre = deezerApiClient.ToDisplayString(gameSession.Room.Genre),
            WinnerId = winner.UserId,
            WinnerName = winner.User.Username,
            Scores = scores
        };
    }
}
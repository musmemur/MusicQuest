using Backend.Api.Services;
using Backend.Domain.Abstractions;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Hubs;

public class QuizHub(
    GameSessionService gameSessionService,
    QuizQuestionService quizQuestionService,
    IGameSessionRepository gameSessionRepository,
    RoomService roomService,
    IRoomRepository roomRepository,
    ILogger<QuizHub> logger,
    PlaylistService playlistService) : Hub
{
    public async Task JoinRoom(string roomId, string userId)
    {
        try
        {
            var (room, user, isNewPlayer) = await roomService.JoinRoomAsync(roomId, userId);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            var players = await roomService.GetRoomPlayersAsync(roomId);
            
            await Clients.Caller.SendAsync("ReceivePlayersList", players);
            
            if (isNewPlayer)
            {
                await Clients.OthersInGroup(roomId).SendAsync("PlayerJoined", new 
                {
                    UserId = userId,
                    user.Username,
                    user.UserPhoto,
                    Score = 0
                });
            }
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Error joining room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error joining room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Internal server error");
        }
    }

    public async Task StartGame(string roomId)
    {
        logger.LogInformation("Starting game for room {RoomId}", roomId);
    
        try 
        {
            var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));
            if (room == null)
            {
                logger.LogWarning("Room not found: {RoomId}", roomId);
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            logger.LogDebug("Generating {Count} questions for genre {Genre}", 
                room.QuestionsCount, room.Genre);
            
            var questions = await quizQuestionService.GenerateQuestionsAsync(room.Genre, room.QuestionsCount);
            var gameSessionId = await gameSessionService.StartNewSessionAsync(roomId, questions);

            logger.LogInformation("Game session {SessionId} started, notifying {PlayerCount} players", 
                gameSessionId, room.Players.Count);

            // Явно проверяем подключенных клиентов
            var group = Clients.Group(roomId);
            await group.SendAsync("GameStarted", gameSessionId);
            logger.LogInformation("GameStarted notification sent for session {SessionId}", gameSessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting game for room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Failed to start game");
        }
    }    
    public async Task GetNextQuestion(string gameSessionId)
    {
        var question = await gameSessionService.GetNextQuestionAsync(gameSessionId);
        if (question == null)
        {
            await EndGame(gameSessionId);
            return;
        }

        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));
        await Clients.Group(gameSession.RoomId.ToString()).SendAsync("NextQuestion", new
        {
            question.Id,
            question.QuestionText,
            question.QuestionType,
            question.CorrectAnswer,
            question.Options,
            question.CorrectIndex,
            question.PreviewUrl,
            question.CoverUrl,
            QuestionIndex = gameSession.CurrentQuestionIndex - 1,
            TotalQuestions = gameSession.QuestionsCount
        });
    }

    public async Task<int> SubmitAnswer(string userId, string gameSessionId, int answerIndex, int questionIndex, int remainingTime)
    {
        return await gameSessionService.ProcessAnswerAsync(
            userId, gameSessionId, answerIndex, questionIndex, remainingTime);
    }

    public async Task GetGameResults(string gameSessionId)
    {
        var results = await gameSessionService.PrepareGameResultsAsync(gameSessionId);
        await Clients.Group(results.RoomId.ToString()).SendAsync("ReceiveGameResults", results);
    }
    
    public async Task EndGame(string gameSessionId)
    {
        var results = await gameSessionService.PrepareGameResultsAsync(gameSessionId);
        await playlistService.CreateWinnerPlaylistAsync(
            gameSessionId, results.WinnerId.ToString(), Enum.Parse<DeezerGenre>(results.Genre));

        await Clients.Group(results.RoomId.ToString()).SendAsync("GameEnded", results.Scores);
        await gameSessionService.EndSessionAsync(gameSessionId);
    }

    public async Task IsUserHost(string gameSessionId, string userId)
    {
        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));
        var isHost = gameSession?.Room.HostUserId.ToString() == userId;
        await Clients.Caller.SendAsync("ReceiveHostStatus", isHost);
    }
    
    public override async Task OnConnectedAsync() => await base.OnConnectedAsync();
    public override async Task OnDisconnectedAsync(Exception? exception) => await base.OnDisconnectedAsync(exception);
}
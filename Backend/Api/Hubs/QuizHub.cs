using Backend.DataBase;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Models;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Hubs;

public class QuizHub(
    DeezerApiClient deezerClient,
    AppDbContext dbContext,
    IGameSessionRepository gameSessionRepository,
    IPlayerRepository playerRepository,
    IUserRepository userRepository,
    IRoomRepository roomRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ITrackRepository trackRepository,
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ILogger<QuizHub> logger, DeezerApiClient deezerApiClient) : Hub
{
    public async Task JoinRoom(string roomId, string userId)
    {
        logger.LogInformation("Пользователь {UserId} пытается присоединиться к комнате {RoomId}", userId, roomId);

        var roomExists = await roomRepository.GetByIdAsync(Guid.Parse(roomId));
        if (roomExists == null)
        {
            logger.LogWarning("Комната {RoomId} не найдена", roomId);
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        var user = await userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            logger.LogWarning("Пользователь {UserId} не найден", userId);
            await Clients.Caller.SendAsync("Error", "User not found");
            return;
        }
        
        var alreadyJoined = await playerRepository.PlayerExistsInRoomAsync(Guid.Parse(userId), Guid.Parse(roomId));
        if (!alreadyJoined)
        {
            logger.LogDebug("Создание нового игрока для пользователя {UserId} в комнате {RoomId}", userId, roomId);
            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                RoomId = Guid.Parse(roomId),
                Score = 0
            };

            await playerRepository.AddAsync(player);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        logger.LogDebug("Пользователь {UserId} добавлен в группу комнаты {RoomId}", userId, roomId);
        
        var players = await playerRepository.GetPlayersByRoomAsync(Guid.Parse(roomId));
        var playerDtos = players.Select(p => new {
            UserId = p.UserId.ToString(),
            p.User.Username,
            p.User.UserPhoto,
            p.Score
        }).ToList();
        
        await Clients.Caller.SendAsync("ReceivePlayersList", playerDtos);
    
        await Clients.OthersInGroup(roomId).SendAsync("PlayerJoined", new {
            UserId = userId,
            Username = user.Username,
            UserPhoto = user.UserPhoto,
            Score = 0
        });

        logger.LogInformation("Пользователь {Username} ({UserId}) успешно присоединился к комнате {RoomId}", 
            user.Username, userId, roomId);
    }

    public async Task StartGame(string roomId)
    {
        logger.LogInformation("Начало игры в комнате {RoomId}", roomId);

        var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));

        if (room == null)
        {
            logger.LogWarning("Комната {RoomId} не найдена при запуске игры", roomId);
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        logger.LogDebug("Генерация {QuestionCount} вопросов для комнаты {RoomId}", room.QuestionsCount, roomId);
        var questions = new List<QuizQuestion>();
        for (var i = 0; i < room.QuestionsCount; i++)
        {
            var questionType = (i % 2 == 0) ? "artist" : "track";
            try
            {
                var question = await deezerClient.GenerateQuizQuestionAsync(room.Genre, questionType);
                questions.Add(question);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при генерации вопроса {Index} для комнаты {RoomId}", i, roomId);
                throw;
            }
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
        logger.LogInformation("Игровая сессия {GameSessionId} создана для комнаты {RoomId}", gameSession.Id, roomId);
        
        await Clients.Group(roomId).SendAsync("GameStarted", gameSession.Id.ToString());
    }

    private async Task SendQuestionToGroup(Guid roomId, Guid gameSessionId)
    {
        logger.LogDebug("Отправка вопроса для сессии {GameSessionId} в комнате {RoomId}", gameSessionId, roomId);

        var gameSession = await gameSessionRepository.GetWithQuestionsAsync(gameSessionId);

        if (gameSession != null && gameSession.CurrentQuestionIndex >= gameSession.Questions.Count)
        {
            logger.LogInformation("Все вопросы отправлены, завершение игры {GameSessionId}", gameSessionId);
            await EndGame(gameSessionId.ToString());
            return;
        }
        
        if (gameSession == null)
        {
            logger.LogWarning("Игровая сессия {GameSessionId} не найдена", gameSessionId);
            await Clients.Caller.SendAsync("Error", "Game session not found");
            return;
        }

        var question = gameSession.Questions[gameSession.CurrentQuestionIndex];
        var currentIndex = gameSession.CurrentQuestionIndex;
        
        ++gameSession.CurrentQuestionIndex;
        await gameSessionRepository.UpdateAsync(gameSession);
        
        logger.LogDebug("Отправка вопроса {Index} для сессии {GameSessionId}", currentIndex, gameSessionId);
        
        await Clients.Group(roomId.ToString()).SendAsync("NextQuestion", new 
        {
            Id = question.Id.ToString(),
            question.QuestionText,
            question.QuestionType,
            question.CorrectAnswer,
            question.Options,
            question.CorrectIndex,
            question.PreviewUrl,
            question.CoverUrl,
            TrackTitle = question.QuestionType == "artist" ? question.CorrectAnswer : null,
            Artist = question.QuestionType == "track" ? question.CorrectAnswer : null,
            QuestionIndex = currentIndex,
            TotalQuestions = gameSession.QuestionsCount
        });
    }

    public async Task GetNextQuestion(string gameSessionId)
    {
        logger.LogDebug("Запрос следующего вопроса для сессии {GameSessionId}", gameSessionId);

        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            logger.LogWarning("Сессия {GameSessionId} не найдена при запросе вопроса", gameSessionId);
            await Clients.Caller.SendAsync("Error", "Game session not found");
            return;
        }

        await SendQuestionToGroup(gameSession.RoomId, gameSession.Id);
    }

    public async Task<int> SubmitAnswer(string userId, string gameSessionId, int answerIndex, int questionIndex, int remainingTime)
    {
        logger.LogDebug("Пользователь {UserId} отправляет ответ на вопрос {QuestionIndex} в сессии {GameSessionId}", 
            userId, questionIndex, gameSessionId);

        try
        {
            var gameSession = await gameSessionRepository.GetWithQuestionsAsync(Guid.Parse(gameSessionId));

            if (gameSession == null)
            {
                logger.LogWarning("Сессия {GameSessionId} не найдена при обработке ответа", gameSessionId);
                throw new HubException("Game session not found");
            }

            if (questionIndex >= gameSession.Questions.Count)
            {
                logger.LogWarning("Неверный индекс вопроса {QuestionIndex} для сессии {GameSessionId}", 
                    questionIndex, gameSessionId);
                throw new HubException("Question index out of range");
            }

            var question = gameSession.Questions[questionIndex];

            var player = await playerRepository.GetPlayerInRoomAsync(Guid.Parse(userId), gameSession.RoomId);

            if (player == null)
            {
                logger.LogWarning("Игрок {UserId} не найден в сессии {GameSessionId}", userId, gameSessionId);
                throw new HubException("Player not found in this game session");
            }

            var isCorrect = answerIndex == question.CorrectIndex;
            if (isCorrect)
            {
                player.Score += remainingTime;
                await playerRepository.UpdateAsync(player);
                logger.LogDebug("Пользователь {UserId} ответил правильно, новый счет: {Score}", 
                    userId, player.Score);
            }
            else
            {
                logger.LogDebug("Пользователь {UserId} ответил неправильно", userId);
            }

            return player.Score;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке ответа от пользователя {UserId}", userId);
            throw new HubException(ex.Message);
        }
    }

    public async Task GetGameResults(string gameSessionId)
    {
        logger.LogInformation("Запрос результатов игры {GameSessionId}", gameSessionId);

        var gameSession = await gameSessionRepository.GetWithRoomAndPlayersAsync(Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            logger.LogWarning("Сессия {GameSessionId} не найдена при запросе результатов", gameSessionId);
            throw new HubException("Game session not found");
        }

        var winner = gameSession.Room.Players
            .OrderByDescending(p => p.Score)
            .First();

        logger.LogDebug("Определен победитель {WinnerId} с счетом {Score}", 
            winner.UserId, winner.Score);

        var scores = gameSession.Room.Players
            .ToDictionary(
                p => p.UserId.ToString(),
                p => new PlayerScoreDto
                {
                    Username = p.User.Username,
                    UserPhoto = p.User.UserPhoto,
                    Score = p.Score
                });

        var results = new GameResultDto
        {
            GameId = gameSession.Id,
            RoomId = gameSession.RoomId,
            Genre = deezerApiClient.ToDisplayString(gameSession.Room.Genre),
            WinnerId = winner.UserId,
            WinnerName = winner.User.Username,
            Scores = scores
        };
        
        await Clients.Group(gameSession.RoomId.ToString()).SendAsync("ReceiveGameResults", results);
        
        await playerRepository.RemoveRangeAsync(gameSession.Room.Players);
        logger.LogInformation("Результаты игры {GameSessionId} отправлены, игроки удалены", gameSessionId);
    }

    private async Task CreateWinnerPlaylist(string gameSessionId, string winnerId, DeezerGenre genre)
    {
        logger.LogInformation("Создание плейлиста для победителя {WinnerId} игры {GameSessionId}", 
            winnerId, gameSessionId);

        var gameSessionGuid = Guid.Parse(gameSessionId);
        var winnerIdGuid = Guid.Parse(winnerId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            if (await playlistRepository.ExistsForGameSessionAsync(gameSessionGuid))
            {
                logger.LogDebug("Плейлист для сессии {GameSessionId} уже существует", gameSessionId);
                await transaction.CommitAsync();
                return;
            }

            var tracks = await deezerClient.GetTracksByGenreAsync(genre);
            logger.LogDebug("Получено {TrackCount} треков жанра {Genre}", tracks.Count, genre);
            
            var playlist = new Playlist(winnerIdGuid, $"Playlist of {genre} Music")
            {
                GameSessionId = gameSessionGuid
            };
            
            await playlistRepository.AddAsync(playlist);
            
            var trackEntities = new List<Track>();
            var newTracks = new List<Track>();
            
            foreach (var deezerTrack in tracks)
            {
                var existingTrack = await trackRepository.GetByDeezerIdAsync(deezerTrack.Id);
                
                if (existingTrack == null)
                {
                    existingTrack = new Track
                    {
                        Id = Guid.NewGuid(),
                        DeezerTrackId = deezerTrack.Id,
                        Title = deezerTrack.Title,
                        Artist = deezerTrack.Artist.Name,
                        PreviewUrl = deezerTrack.Preview,
                        CoverUrl = deezerTrack.Album.Cover
                    };
                    newTracks.Add(existingTrack);
                }
                trackEntities.Add(existingTrack);
            }

            if (newTracks.Count != 0)
            {
                logger.LogDebug("Добавление {NewTrackCount} новых треков", newTracks.Count);
                await trackRepository.AddRangeAsync(newTracks);
            }

            var playlistTracks = trackEntities.Select(track => new PlaylistTrack
            {
                PlaylistId = playlist.Id,
                TrackId = track.Id,
                Track = track
            }).ToList();

            await playlistTrackRepository.AddRangeAsync(playlistTracks);
            logger.LogInformation("Плейлист {PlaylistId} создан с {TrackCount} треками", 
                playlist.Id, playlistTracks.Count);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании плейлиста для победителя");
            await transaction.RollbackAsync();
            throw new HubException($"Failed to create playlist: {ex.Message}");
        }
    }

    public async Task EndGame(string gameSessionId)
    {
        logger.LogInformation("Завершение игры {GameSessionId}", gameSessionId);

        var gameSession = await gameSessionRepository.GetWithRoomPlayersAndQuestionsAsync(Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            logger.LogWarning("Сессия {GameSessionId} не найдена при завершении игры", gameSessionId);
            return;
        }

        var winner = gameSession.Room.Players
            .OrderByDescending(p => p.Score)
            .First();

        logger.LogDebug("Победитель игры {GameSessionId}: {WinnerId} ({Score} очков)", 
            gameSessionId, winner.UserId, winner.Score);

        await CreateWinnerPlaylist(gameSessionId, winner.UserId.ToString(), gameSession.Room.Genre);

        await Clients.Group(gameSession.Room.Id.ToString()).SendAsync("GameEnded", new
        {
            Scores = gameSession.Room.Players.ToDictionary(
                p => p.UserId.ToString(),
                p => new { p.User.Username, p.User.UserPhoto, p.Score })
        });

        await quizQuestionRepository.RemoveRangeAsync(gameSession.Questions);
        await gameSessionRepository.EndGameSessionAsync(gameSession.Id);
        logger.LogInformation("Игра {GameSessionId} завершена, ресурсы очищены", gameSessionId);
    }

    public async Task IsUserHost(string gameSessionId, string userId)
    {
        logger.LogDebug("Проверка, является ли пользователь {UserId} хостом сессии {GameSessionId}", 
            userId, gameSessionId);

        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));
        
        if (gameSession == null)
        {
            logger.LogWarning("Сессия {GameSessionId} не найдена при проверке хоста", gameSessionId);
            throw new HubException("Game session not found");
        }

        var isHost = gameSession.Room.HostUserId.ToString() == userId;
        logger.LogDebug("Пользователь {UserId} {IsHost} хостом сессии {GameSessionId}", 
            userId, isHost ? "является" : "не является", gameSessionId);
        await Clients.Caller.SendAsync("ReceiveHostStatus", isHost);
    }
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
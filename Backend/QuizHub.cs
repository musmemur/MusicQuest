using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using Backend.DataBase;
using Backend.Entities;
using Backend.Modals;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Backend;

public class QuizHub(DeezerApiClient deezerClient, 
    AppDbContext dbContext, 
    UserService userService) : Hub
{
    public async Task JoinRoom(string roomId, string userId)
    {
        var roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == Guid.Parse(roomId));
        if (!roomExists)
        {
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
        
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found");
            return;
        }
        
        var alreadyJoined = await dbContext.Players
            .AnyAsync(p => p.RoomId == Guid.Parse(roomId) && p.UserId == Guid.Parse(userId));

        if (!alreadyJoined)
        {
            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                RoomId = Guid.Parse(roomId),
                Score = 0,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync();
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var players = await dbContext.Players
            .Where(p => p.RoomId == Guid.Parse(roomId))
            .Select(p => new {
                p.UserId,
                p.User.Username,
                p.User.UserPhoto,
                p.Score
            })
            .ToListAsync();
        
        await Clients.Caller.SendAsync("ReceivePlayersList", players);
    
        await Clients.OthersInGroup(roomId).SendAsync("PlayerJoined", new {
            UserId = userId,
            Username = user.Username,
            UserPhoto = user.UserPhoto,
            Score = 0
        });
    }
    
    public async Task StartGame(string roomId)
    {
        var room = await dbContext.Rooms
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Id == Guid.Parse(roomId));

        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        var questions = new List<QuizQuestion>();
        for (var i = 0; i < room.QuestionsCount; i++)
        {
            var questionType = (i % 2 == 0) ? "artist" : "track";
            var question = await deezerClient.GenerateQuizQuestionAsync(room.Genre, questionType);
            questions.Add(question);
        }

        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = "InProgress",
            StartedAt = DateTime.UtcNow,
            RoomId = room.Id,
            Questions = questions,
            QuestionsCount = room.QuestionsCount,
            CurrentQuestionIndex = 0
        };

        dbContext.GameSessions.Add(gameSession);
        await dbContext.SaveChangesAsync();
        
        await Clients.Group(roomId).SendAsync("GameStarted", gameSession.Id.ToString());
    } 
    
    private async Task SendQuestionToGroup(Guid roomId, Guid gameSessionId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Questions)
            .FirstOrDefaultAsync(gs => gs.Id == gameSessionId);

        if (gameSession != null && gameSession.CurrentQuestionIndex >= gameSession.Questions.Count)
        {
            await EndGame(gameSessionId.ToString());
            return;
        }

        var question = gameSession.Questions[gameSession.CurrentQuestionIndex];
        var currentIndex = gameSession.CurrentQuestionIndex;
        
        ++gameSession.CurrentQuestionIndex;
        await dbContext.SaveChangesAsync();
        
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
    
    public async Task UpdatePlayerCount(string roomId, int newCount)
    {
        // Обновляем данные комнаты
        // И рассылаем обновление всем клиентам
        await Clients.All.SendAsync("PlayerCountChanged", roomId, newCount);
    }
    
    public async Task GetNextQuestion(string gameSessionId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .Include(gs => gs.Questions)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            await Clients.Caller.SendAsync("Error", "Game session not found");
            return;
        }

        await SendQuestionToGroup(gameSession.RoomId, gameSession.Id);
    }
    
    public async Task<int> SubmitAnswer(string userId, string gameSessionId, int answerIndex, int questionIndex, int remainingTime)
    {
        try
        {
            // Get the game session with questions
            var gameSession = await dbContext.GameSessions
                .Include(gs => gs.Questions)
                .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

            if (gameSession == null)
            {
                throw new HubException("Game session not found");
            }

            if (questionIndex >= gameSession.Questions.Count)
            {
                throw new HubException("Question index out of range");
            }

            var question = gameSession.Questions[questionIndex];

            var player = await dbContext.Players
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userId) && p.RoomId == gameSession.RoomId);

            if (player == null)
            {
                throw new HubException("Player not found in this game session");
            }

            // Check if answer is correct
            var isCorrect = answerIndex == question.CorrectIndex;

            // Update player score if answer is correct
            if (!isCorrect) return player.Score;
            player.Score += (remainingTime * 3);
            await dbContext.SaveChangesAsync();

            return player.Score;
        }
        catch (Exception ex)
        {
            // Log the error
            await Console.Error.WriteLineAsync($"Error in SubmitAnswer: {ex}");
            throw new HubException(ex.Message);
        }
    } 
    
    public async Task<GameResultDto> GetGameResults(string gameSessionId)
    {
        // Сначала попробуем найти существующие результаты
        var gameResult = await dbContext.GameResults
            .Include(gr => gr.PlayerScores)
            .ThenInclude(ps => ps.User)
            .Include(gr => gr.Winner)
            .FirstOrDefaultAsync(gr => gr.GameSessionId == Guid.Parse(gameSessionId));

        if (gameResult != null)
        {
            return new GameResultDto
            {
                GameId = gameResult.GameSessionId,
                RoomId = gameResult.RoomId,
                Genre = gameResult.Genre,
                WinnerId = gameResult.WinnerId,
                WinnerName = gameResult.Winner.Username,
                PlaylistId = gameResult.PlaylistId?.ToString(),
                Scores = gameResult.PlayerScores.ToDictionary(
                    ps => ps.UserId.ToString(),
                    ps => new PlayerScoreDto
                    {
                        Username = ps.User.Username,
                        UserPhoto = ps.User.UserPhoto,
                        Score = ps.Score
                    }),
            };
        }

        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .ThenInclude(r => r.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            throw new HubException("Game session not found");
        }

        var winner = gameSession.Room.Players
            .OrderByDescending(p => p.Score)
            .First();

        var scores = gameSession.Room.Players
            .ToDictionary(
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
            Genre = gameSession.Room.Genre,
            WinnerId = winner.UserId,
            WinnerName = winner.User.Username,
            Scores = scores
        };
    }

    public async Task<string> CreateWinnerPlaylist(string gameSessionId, string winnerId, string genre)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null)
        {
            throw new HubException("Game session not found");
        }

        var tracks = await deezerClient.GetTracksByGenreAsync(genre, 15);

        var playlist = new Playlist(Guid.Parse(winnerId), $"Quiz Winner: {genre} Music");
        
        foreach (var deezerTrack in tracks)
        {
            var existingTrack = await dbContext.Tracks
                .FirstOrDefaultAsync(t => t.DeezerTrackId == deezerTrack.Id);
            
            if (existingTrack == null)
            {
                existingTrack = new Track
                {
                    Id = Guid.NewGuid(),
                    DeezerTrackId = deezerTrack.Id,
                    Title = deezerTrack.Title,
                    Artist = deezerTrack.Artist?.Name ?? "Unknown Artist",
                    PreviewUrl = deezerTrack.Preview,
                    CoverUrl = deezerTrack.Album?.Cover ?? string.Empty
                };
                dbContext.Tracks.Add(existingTrack);
            }

            var playlistTrack = new PlaylistTrack
            {
                PlaylistId = playlist.Id,
                TrackId = existingTrack.Id
            };
            playlist.PlaylistTracks.Add(playlistTrack);
        }

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        return playlist.Id.ToString();
    }
    
    public async Task EndGame(string gameSessionId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .ThenInclude(r => r.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null) return;

        var winner = gameSession.Room.Players
            .OrderByDescending(p => p.Score)
            .First();

        var playlistId = await CreateWinnerPlaylist(gameSessionId, winner.UserId.ToString(), gameSession.Room.Genre);

        await Clients.Group(gameSession.Room.Id.ToString()).SendAsync("GameEnded", new
        {
            GameId = gameSession.Id,
            WinnerId = winner.UserId,
            Scores = gameSession.Room.Players.ToDictionary(
                p => p.UserId.ToString(),
                p => new { p.User.Username, p.User.UserPhoto, p.Score }),
            Genre = gameSession.Room.Genre,
            PlaylistId = playlistId
        });

        gameSession.Room.IsActive = false;
        await dbContext.SaveChangesAsync();
    }
    
    // public override async Task OnDisconnectedAsync(Exception? exception)
    // {
    //     var connectionId = Context.ConnectionId;
    //
    //     var playerConnection = await dbContext.PlayerConnections
    //         .Include(pc => pc.Player)
    //         .FirstOrDefaultAsync(pc => pc.ConnectionId == connectionId);
    //
    //     if (playerConnection != null)
    //     {
    //         var player = playerConnection.Player;
    //         var roomId = player.RoomId.ToString();
    //     
    //         // Удаляем связь соединения с игроком
    //         dbContext.PlayerConnections.Remove(playerConnection);
    //     
    //         // Проверяем, есть ли у игрока другие активные соединения
    //         var hasOtherConnections = await dbContext.PlayerConnections
    //             .AnyAsync(pc => pc.PlayerId == player.Id);
    //         
    //         if (!hasOtherConnections)
    //         {
    //             // Удаляем игрока, если это его последнее соединение
    //             dbContext.Players.Remove(player);
    //             await Clients.Group(roomId).SendAsync("PlayerLeft", player.UserId.ToString());
    //         }
    //     
    //         await dbContext.SaveChangesAsync();
    //     }
    //
    //     await base.OnDisconnectedAsync(exception);
    // }
}

public class GameResultDto
{
    public Guid GameId { get; set; }
    public Guid RoomId { get; set; }
    public string Genre { get; set; }
    public Guid WinnerId { get; set; }
    public string WinnerName { get; set; }
    public string PlaylistId { get; set; }
    public Dictionary<string, PlayerScoreDto> Scores { get; set; }
}

public class PlayerScoreDto
{
    public string Username { get; set; }
    public string UserPhoto { get; set; }
    public int Score { get; set; }
}
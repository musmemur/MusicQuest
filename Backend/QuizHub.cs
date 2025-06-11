using Microsoft.AspNetCore.SignalR;
using Backend.DataBase;
using Backend.Dto;
using Backend.Entities;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class QuizHub(DeezerApiClient deezerClient, 
    AppDbContext dbContext) : Hub
{
    public async Task JoinRoom(string roomId, string userId)
    {
        var roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == Guid.Parse(roomId));
        if (!roomExists)
        {
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        //надо переделать передачу данных о пользователе
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
        
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found");
            return;
        }
        
        //проверить на это
        var alreadyJoined = await dbContext.Players
            .AnyAsync(p => p.RoomId == Guid.Parse(roomId) && p.UserId == Guid.Parse(userId));

        if (!alreadyJoined)
        {
            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                RoomId = Guid.Parse(roomId),
                Score = 0
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
        
        if (gameSession == null)
        {
            await Clients.Caller.SendAsync("Error", "Game session not found");
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

            var isCorrect = answerIndex == question.CorrectIndex;

            if (!isCorrect) return player.Score;
            player.Score += remainingTime;
            await dbContext.SaveChangesAsync();

            return player.Score;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    } 
    
    //надо будет здесь удалять игроков + добавить, чтобы вызывался метод один раз хостом
    public async Task GetGameResults(string gameSessionId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .ThenInclude(r => r.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null)
            throw new HubException("Game session not found");

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

        var results = new GameResultDto
        {
            GameId = gameSession.Id,
            RoomId = gameSession.RoomId,
            Genre = gameSession.Room.Genre,
            WinnerId = winner.UserId,
            WinnerName = winner.User.Username,
            Scores = scores
        };
        
        await Clients.Group(gameSession.RoomId.ToString()).SendAsync("ReceiveGameResults", results);
        
        dbContext.Players.RemoveRange(gameSession.Room.Players);
        dbContext.GameSessions.Remove(gameSession);
        await dbContext.SaveChangesAsync();
    }

    public async Task CreateWinnerPlaylist(string gameSessionId, string winnerId, string genre)
    {
        var gameSessionGuid = Guid.Parse(gameSessionId);
        var winnerIdGuid = Guid.Parse(winnerId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingPlaylist = await dbContext.Playlists
                .FirstOrDefaultAsync(p => p.GameSessionId == gameSessionGuid);

            if (existingPlaylist != null)
            {
                await transaction.CommitAsync();
            }

            var tracks = await deezerClient.GetTracksByGenreAsync(genre);

            var playlist = new Playlist(winnerIdGuid, $"Playlist of {genre} Music")
            {
                GameSessionId = gameSessionGuid,
                PlaylistTracks = []
            };

            var trackEntities = new List<Track>();
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
                        Artist = deezerTrack.Artist.Name,
                        PreviewUrl = deezerTrack.Preview,
                        CoverUrl = deezerTrack.Album.Cover
                    };
                    dbContext.Tracks.Add(existingTrack);
                }
                trackEntities.Add(existingTrack);
            }

            await dbContext.SaveChangesAsync();

            foreach (var track in trackEntities)
            {
                playlist.PlaylistTracks.Add(new PlaylistTrack
                {
                    TrackId = track.Id,
                    Track = track
                });
            }

            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task EndGame(string gameSessionId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .ThenInclude(r => r.Players)
            .ThenInclude(p => p.User)
            .Include(gs => gs.Questions) // Добавляем включение вопросов
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));

        if (gameSession == null) return;

        var winner = gameSession.Room.Players
            .OrderByDescending(p => p.Score)
            .First();

        await CreateWinnerPlaylist(gameSessionId, winner.UserId.ToString(), gameSession.Room.Genre);

        await Clients.Group(gameSession.Room.Id.ToString()).SendAsync("GameEnded", new
        {
            Scores = gameSession.Room.Players.ToDictionary(
                p => p.UserId.ToString(),
                p => new { p.User.Username, p.User.UserPhoto, p.Score })
        });

        dbContext.QuizQuestions.RemoveRange(gameSession.Questions);
        gameSession.Status = "Completed";
        gameSession.Room.IsActive = false;
        await dbContext.SaveChangesAsync();
    }
    
    public async Task IsUserHost(string gameSessionId, string userId)
    {
        var gameSession = await dbContext.GameSessions
            .Include(gs => gs.Room)
            .Include(gs => gs.Questions)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId));
        
        if (gameSession == null)
        {
            throw new HubException("Game session not found");
        }

        var isHost = gameSession.Room.HostUserId.ToString() == userId;
        await Clients.Caller.SendAsync("ReceiveHostStatus", isHost);
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
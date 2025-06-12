using Microsoft.AspNetCore.SignalR;
using Backend.DataBase;
using Backend.Dto;
using Backend.Entities;
using Backend.Repositories;
using Backend.Services;

namespace Backend;

public class QuizHub(DeezerApiClient deezerClient, AppDbContext dbContext,
    IGameSessionRepository gameSessionRepository, IPlayerRepository playerRepository,
    IUserRepository userRepository, IRoomRepository roomRepository, 
    IQuizQuestionRepository quizQuestionRepository, ITrackRepository trackRepository,
    IPlaylistRepository playlistRepository, IPlaylistTrackRepository playlistTrackRepository) : Hub
{
    public async Task JoinRoom(string roomId, string userId)
    {
        var roomExists = await roomRepository.GetByIdAsync(Guid.Parse(roomId));
        if (roomExists == null)
        {
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        var user = await userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found");
            return;
        }
        
        var alreadyJoined = await playerRepository.PlayerExistsInRoomAsync(Guid.Parse(userId), Guid.Parse(roomId));
        if (!alreadyJoined)
        {
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
    }   
    public async Task StartGame(string roomId)
    {
        var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));

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

        await gameSessionRepository.AddAsync(gameSession);
        
        await Clients.Group(roomId).SendAsync("GameStarted", gameSession.Id.ToString());
    } 
    
    private async Task SendQuestionToGroup(Guid roomId, Guid gameSessionId)
    {
        var gameSession = await gameSessionRepository.GetWithQuestionsAsync(gameSessionId);

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
        await gameSessionRepository.UpdateAsync(gameSession);
        
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
        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));

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
            var gameSession = await gameSessionRepository.GetWithQuestionsAsync(Guid.Parse(gameSessionId));

            if (gameSession == null)
            {
                throw new HubException("Game session not found");
            }

            if (questionIndex >= gameSession.Questions.Count)
            {
                throw new HubException("Question index out of range");
            }

            var question = gameSession.Questions[questionIndex];

            var player = await playerRepository.GetPlayerInRoomAsync(Guid.Parse(userId), gameSession.RoomId);

            if (player == null)
            {
                throw new HubException("Player not found in this game session");
            }

            var isCorrect = answerIndex == question.CorrectIndex;
            if (isCorrect)
            {
                player.Score += remainingTime;
                await playerRepository.UpdateAsync(player);
            }

            return player.Score;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    } 
    public async Task GetGameResults(string gameSessionId)
    {
        var gameSession = await gameSessionRepository.GetWithRoomAndPlayersAsync(Guid.Parse(gameSessionId));

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
        
        await playerRepository.RemoveRangeAsync(gameSession.Room.Players);
    }

    private async Task CreateWinnerPlaylist(string gameSessionId, string winnerId, DeezerGenre genre)
    {
        var gameSessionGuid = Guid.Parse(gameSessionId);
        var winnerIdGuid = Guid.Parse(winnerId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            if (await playlistRepository.ExistsForGameSessionAsync(gameSessionGuid))
            {
                await transaction.CommitAsync();
                return;
            }

            var tracks = await deezerClient.GetTracksByGenreAsync(genre);
            
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
                await trackRepository.AddRangeAsync(newTracks);
            }

            var playlistTracks = trackEntities.Select(track => new PlaylistTrack
            {
                PlaylistId = playlist.Id,
                TrackId = track.Id,
                Track = track
            }).ToList();

            await playlistTrackRepository.AddRangeAsync(playlistTracks);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new HubException($"Failed to create playlist: {ex.Message}");
        }
    }    
    public async Task EndGame(string gameSessionId)
    {
        var gameSession = await gameSessionRepository.GetWithRoomPlayersAndQuestionsAsync(Guid.Parse(gameSessionId));

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

        await quizQuestionRepository.RemoveRangeAsync(gameSession.Questions);
        await gameSessionRepository.EndGameSessionAsync(gameSession.Id);
    }
    
    public async Task IsUserHost(string gameSessionId, string userId)
    {
        var gameSession = await gameSessionRepository.GetWithRoomAndQuestionsAsync(Guid.Parse(gameSessionId));
        
        if (gameSession == null)
        {
            throw new HubException("Game session not found");
        }

        var isHost = gameSession.Room.HostUserId.ToString() == userId;
        await Clients.Caller.SendAsync("ReceiveHostStatus", isHost);
    }
    
    // public override async Task OnDisconnectedAsync(Exception? exception)
    // {
    //     try
    //     {
    //         var connection = Context;
    //         
    //         var allPlayers = await playerRepository.GetAllAsync();
    //         
    //         var player = allPlayers.FirstOrDefault(p => p.UserId.ToString() == Context.UserIdentifier);
    //         
    //         if (player != null)
    //         {
    //             var roomId = player.RoomId.ToString();
    //             var userId = player.UserId.ToString();
    //             
    //             await playerRepository.RemoveAsync(player);
    //             
    //             await Clients.Group(roomId).SendAsync("GameDisconnected", exception?.Message);
    //             
    //             //await Groups.RemoveFromGroupAsync(connectionI, roomId);
    //         }
    //
    //         await base.OnDisconnectedAsync(exception);
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
    //         await base.OnDisconnectedAsync(exception);
    //     }
    // }
}
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.DataBase;
using Microsoft.Extensions.Logging;
using Moq;


namespace Backend.Application.Tests;

public class GameSessionServiceTests
{
    private readonly Mock<IGameSessionRepository> _gameSessionRepoMock = new();
    private readonly Mock<IPlayerRepository> _playerRepoMock = new();
    private readonly Mock<IRoomRepository> _roomRepoMock = new();
    private readonly Mock<ILogger<IGameSessionService>> _loggerMock = new();
    private readonly Mock<IDeezerApiClient> _deezerApiMock = new();
    private readonly GameSessionService _service;

    public GameSessionServiceTests()
    {
        _service = new GameSessionService(
            _gameSessionRepoMock.Object,
            _playerRepoMock.Object,
            _roomRepoMock.Object,
            _loggerMock.Object,
            _deezerApiMock.Object);
    }

    [Fact]
    public async Task StartNewSessionAsync_ShouldCreateNewSession_WhenRoomExists()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var questions = new List<QuizQuestion> { new() };
        var room = new Room { Id = roomId, Players = new List<Player>() };
        
        _roomRepoMock.Setup(x => x.GetRoomWithPlayersAsync(roomId))
            .ReturnsAsync(room);
        
        // Act
        var result = await _service.StartNewSessionAsync(roomId.ToString(), questions);
        
        // Assert
        Assert.NotNull(result);
        _gameSessionRepoMock.Verify(x => x.AddAsync(It.IsAny<GameSession>()), Times.Once);
    }

    [Fact]
    public async Task PrepareGameResultsAsync_ShouldReturnCorrectWinners_WhenMultiplePlayersHaveMaxScore()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var players = new List<Player>
        {
            new Player { 
                UserId = Guid.NewGuid(), 
                Score = 10, 
                User = new User("User1", "pass1", "photo1") 
            },
            new Player { 
                UserId = Guid.NewGuid(), 
                Score = 10, 
                User = new User("User2", "pass2", "photo2") 
            },
            new Player { 
                UserId = Guid.NewGuid(), 
                Score = 5, 
                User = new User("User3", "pass3", "photo3") 
            }
        };
        
        var gameSession = new GameSession
        {
            Id = sessionId,
            Room = new Room { Players = players, Genre = DeezerGenre.Pop }
        };
        
        _gameSessionRepoMock.Setup(x => x.GetWithRoomPlayersAndQuestionsAsync(sessionId))
            .ReturnsAsync(gameSession);
        
        _deezerApiMock.Setup<string>(x => x.ToDisplayString(It.IsAny<DeezerGenre>()))
            .Returns<string>(_ => "Pop");
        
        // Act
        var result = await _service.PrepareGameResultsAsync(sessionId.ToString());
        
        // Assert
        Assert.Equal(2, result.Winners.Count);
        Assert.Equal(2, result.WinnerNames.Count);
        Assert.Contains("User1", result.WinnerNames);
        Assert.Contains("User2", result.WinnerNames);
    }
}

public class PlaylistServiceTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepoMock = new();
    private readonly Mock<IPlaylistTrackRepository> _playlistTrackRepoMock = new();
    private readonly Mock<ITrackRepository> _trackRepoMock = new();
    private readonly Mock<IDeezerApiClient> _deezerClientMock = new();
    private readonly Mock<AppDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<IPlaylistService>> _loggerMock = new();
    private readonly PlaylistService _service;

    public PlaylistServiceTests()
    {
        _service = new PlaylistService(
            _playlistRepoMock.Object,
            _playlistTrackRepoMock.Object,
            _trackRepoMock.Object,
            _deezerClientMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateWinnerPlaylistAsync_ShouldNotCreatePlaylist_WhenAlreadyExists()
    {
        // Arrange
        var gameSessionId = Guid.NewGuid().ToString();
        var winnerId = Guid.NewGuid().ToString();
        var genre = DeezerGenre.Pop;
        
        _playlistRepoMock.Setup(x => x.ExistsForGameSessionAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        
        // Act
        await _service.CreateWinnerPlaylistAsync(gameSessionId, winnerId, genre);
        
        // Assert
        _playlistRepoMock.Verify(x => x.AddAsync(It.IsAny<Playlist>()), Times.Never);
    }

    [Fact]
    public async Task CreateWinnerPlaylistAsync_ShouldCreatePlaylistWithTracks_WhenNotExists()
    {
        // Arrange
        var gameSessionId = Guid.NewGuid().ToString();
        var winnerId = Guid.NewGuid().ToString();
        var genre = DeezerGenre.Pop;
        var deezerTracks = new List<DeezerTrack>
        {
            new DeezerTrack { 
                Id = 1, 
                Title = "Track1", 
                Artist = new DeezerArtist { Name = "Artist1" },
                Preview = "preview1",
                Album = new DeezerAlbum { Cover = "cover1" }
            }
        };
        
        _playlistRepoMock.Setup(x => x.ExistsForGameSessionAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);
        
        _deezerClientMock.Setup(x => x.GenerateTracksToPlaylistByGenreAsync(genre))
            .ReturnsAsync(deezerTracks);
        
        _trackRepoMock.Setup(x => x.GetByDeezerIdAsync(It.IsAny<long>()))
            .ReturnsAsync((Track)null);
        
        // Act
        await _service.CreateWinnerPlaylistAsync(gameSessionId, winnerId, genre);
        
        // Assert
        _playlistRepoMock.Verify(x => x.AddAsync(It.IsAny<Playlist>()), Times.Once);
        _trackRepoMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Track>>()), Times.Once);
        _playlistTrackRepoMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PlaylistTrack>>()), Times.Once);
    }
}

public class RoomServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepoMock = new();
    private readonly Mock<IPlayerRepository> _playerRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<IRoomService>> _loggerMock = new();
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        _service = new RoomService(
            _roomRepoMock.Object,
            _playerRepoMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task JoinRoomAsync_ShouldAddNewPlayer_WhenNotExists()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User("testuser", "password", "photo.jpg") { Id = userId };
        
        _roomRepoMock.Setup(x => x.GetByIdAsync(roomId))
            .ReturnsAsync(new Room { Id = roomId });
        
        _userRepoMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        _playerRepoMock.Setup(x => x.PlayerExistsInRoomAsync(userId, roomId))
            .ReturnsAsync(false);
        
        // Act
        var result = await _service.JoinRoomAsync(roomId.ToString(), userId.ToString());
        
        // Assert
        Assert.True(result.IsNewPlayer);
        _playerRepoMock.Verify(x => x.AddAsync(It.IsAny<Player>()), Times.Once);
    }

    [Fact]
    public async Task LeaveRoomAsync_ShouldRemoveRoom_WhenLastPlayerLeaves()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var player = new Player { Id = Guid.NewGuid(), UserId = userId };
        var room = new Room { Id = roomId, Players = new List<Player> { player } };
        
        _roomRepoMock.Setup(x => x.GetRoomWithPlayersAsync(roomId))
            .ReturnsAsync(room);
        
        // Act
        var result = await _service.LeaveRoomAsync(roomId.ToString(), userId.ToString());
        
        // Assert
        Assert.True(result);
        _roomRepoMock.Verify(x => x.RemoveAsync(room), Times.Once);
    }

    [Fact]
    public async Task SelectNewHostAsync_ShouldSelectRandomPlayer_WhenCurrentHostLeaves()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var players = new List<Player>
        {
            new() { UserId = Guid.NewGuid() },
            new() { UserId = Guid.NewGuid() }
        };
        var room = new Room { Id = roomId, Players = players };
        
        _roomRepoMock.Setup(x => x.GetRoomWithPlayersAsync(roomId))
            .ReturnsAsync(room);
        
        // Act
        await _service.SelectNewHostAsync(roomId.ToString());
        
        // Assert
        _roomRepoMock.Verify(x => x.UpdateAsync(It.Is<Room>(r => 
            players.Any(p => p.UserId == r.HostUserId))), Times.Once);
    }
}
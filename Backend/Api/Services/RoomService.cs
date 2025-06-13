
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Models;

namespace Backend.Api.Services;

public class RoomService(
    IRoomRepository roomRepository,
    IPlayerRepository playerRepository,
    IUserRepository userRepository,
    ILogger<RoomService> logger)
{
    public async Task<(Room Room, User User, bool IsNewPlayer)> JoinRoomAsync(string roomId, string userId)
    {
        var room = await roomRepository.GetByIdAsync(Guid.Parse(roomId));
        if (room == null)
        {
            logger.LogWarning("Room {RoomId} not found", roomId);
            throw new ArgumentException("Room not found");
        }

        var user = await userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found", userId);
            throw new ArgumentException("User not found");
        }

        var isNewPlayer = !await playerRepository.PlayerExistsInRoomAsync(Guid.Parse(userId), Guid.Parse(roomId));
        if (isNewPlayer)
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

        logger.LogInformation("User {Username} joined room {RoomId}", user.Username, roomId);
        return (room, user, isNewPlayer);
    }

    public async Task<IEnumerable<PlayerDto>> GetRoomPlayersAsync(string roomId)
    {
        var players = await playerRepository.GetPlayersByRoomAsync(Guid.Parse(roomId));
        return players.Select(p => new PlayerDto
        {
            UserId = p.UserId.ToString(),
            Username = p.User.Username,
            UserPhoto = p.User.UserPhoto,
            Score = p.Score
        });
    }
}
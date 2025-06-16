using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class RoomService(
    IRoomRepository roomRepository,
    IPlayerRepository playerRepository,
    IUserRepository userRepository,
    ILogger<IRoomService> logger) : IRoomService
{
    public async Task<(User User, bool IsNewPlayer)> JoinRoomAsync(string roomId, string userId)
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
        return (user, isNewPlayer);
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
    
    public async Task LeaveRoomAsync(string roomId, string userId)
    {
        var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));
        if (room == null)
        {
            logger.LogWarning("Room {RoomId} not found", roomId);
            throw new ArgumentException("Room not found");
        }

        var player = room.Players.FirstOrDefault(p => p.UserId == Guid.Parse(userId));
        if (player == null)
        {
            logger.LogWarning("Player {UserId} not found in room {RoomId}", userId, roomId);
            throw new ArgumentException("Player not in room");
        }

        await playerRepository.RemoveAsync(player);
        logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);

        if (room.Players.All(p => p.Id == player.Id)) 
        {
            room.IsActive = false;
            await roomRepository.UpdateAsync(room);
            logger.LogInformation("Room {RoomId} closed (no players left)", roomId);
        }
    }
    
    public async Task SelectNewHostAsync(string roomId)
    {
        var room = await roomRepository.GetRoomWithPlayersAsync(Guid.Parse(roomId));
        if (room == null || room.Players.Count == 0)
        {
            logger.LogWarning("No players available to select new host in room {RoomId}", roomId);
        }

        var candidates = room.Players.ToList();

        if (candidates.Count == 0)
            candidates = room.Players.ToList();

        var random = new Random();
        var newHost = candidates[random.Next(candidates.Count)];

        room.HostUserId = newHost.UserId;
        await roomRepository.UpdateAsync(room);

        logger.LogInformation("New host selected in room {RoomId}: {UserId}", 
            roomId, newHost.UserId);
    }
}
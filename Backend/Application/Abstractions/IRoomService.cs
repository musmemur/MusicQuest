using Backend.Application.Models;
using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IRoomService
{ 
     Task<(User User, bool IsNewPlayer)> JoinRoomAsync(string roomId, string userId);
     Task<IEnumerable<PlayerDto>> GetRoomPlayersAsync(string roomId);
}
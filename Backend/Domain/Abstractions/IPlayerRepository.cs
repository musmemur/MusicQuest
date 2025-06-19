using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetPlayersByRoomAsync(Guid roomId);
    Task<Player?> GetPlayerInRoomAsync(Guid userId, Guid roomId);
    Task<bool> PlayerExistsInRoomAsync(Guid userId, Guid roomId);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task RemoveAsync(Player player);
    Task RemoveRangeAsync(IEnumerable<Player> players);
}
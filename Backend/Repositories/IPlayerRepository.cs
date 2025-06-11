using Backend.Entities;

namespace Backend.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetPlayersByRoomAsync(Guid roomId);
    Task<Player?> GetPlayerInRoomAsync(Guid userId, Guid roomId);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<bool> PlayerExistsInRoomAsync(Guid userId, Guid roomId);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task RemoveAsync(Player player);
    Task RemoveRangeAsync(IEnumerable<Player> players);
}
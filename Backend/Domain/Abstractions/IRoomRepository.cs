using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
    Task<IEnumerable<Room>> GetActiveRoomsAsync();
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task RemoveAsync(Room room);
    Task<Room?> GetRoomWithPlayersAsync(Guid roomId);
    Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId);
    Task<Room?> GetRoomByPlayerAsync(Guid userId);
}

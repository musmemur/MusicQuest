using Backend.Domain.Entities;
using Backend.Domain.Models;

namespace Backend.Domain.Abstractions
{
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(Guid id);
        Task<IEnumerable<RoomDto>> GetActiveRoomsAsync();
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task RemoveAsync(Room room);

        Task<Room?> GetRoomWithPlayersAsync(Guid roomId);
        Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId);
    }
}
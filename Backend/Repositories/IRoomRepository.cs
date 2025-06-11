using Backend.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Dto;

namespace Backend.Repositories
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
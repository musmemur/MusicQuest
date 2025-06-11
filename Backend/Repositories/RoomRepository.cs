using Backend.Entities;
using Backend.DataBase;
using Backend.Dto;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class RoomRepository(AppDbContext context) : IRoomRepository
    {
        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await context.Rooms.FindAsync(id);
        }

        public async Task<IEnumerable<RoomDto>> GetActiveRoomsAsync()
        {
            return await context.Rooms
                .Where(r => r.IsActive)
                .Select(r => new RoomDto
                {
                    Id = r.Id.ToString(),
                    Name = r.Name,
                    Genre = r.Genre,
                    PlayersCount = r.Players.Count
                })
                .ToListAsync();
        }

        public async Task AddAsync(Room room)
        {
            await context.Rooms.AddAsync(room);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            context.Rooms.Update(room);
            await context.SaveChangesAsync();
        }
        
        public async Task RemoveAsync(Room room)
        { 
            context.Rooms.Remove(room);
            await context.SaveChangesAsync();
        }

        public async Task<Room?> GetRoomWithPlayersAsync(Guid roomId)
        {
            return await context.Rooms
                .Include(r => r.Players)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId)
        {
            return await context.Players
                .AnyAsync(p => p.RoomId == roomId && p.UserId == userId);
        }
    }
}
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.DataBase.Rooms
{
    public class RoomRepository(AppDbContext context) : IRoomRepository
    {
        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await context.Rooms.FindAsync(id);
        }

        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
        {
            return await context.Rooms
                .Where(r => r.IsActive)
                .Include(r => r.Players) 
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
        
        public async Task<Room?> GetRoomByPlayerAsync(Guid userId)
        {
            return await context.Rooms
                .Include(r => r.Players)
                .ThenInclude(p => p.User)
                .Where(r => r.IsActive && r.Players.Any(p => p.UserId == userId))
                .FirstOrDefaultAsync();
        }
    }
}
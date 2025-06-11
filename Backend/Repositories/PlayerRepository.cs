using Backend.DataBase;
using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PlayerRepository(AppDbContext dbContext): IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(Guid id)
    {
        return await dbContext.Players
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        
    }

    public async Task<IEnumerable<Player>> GetPlayersByRoomAsync(Guid roomId)
    {
        return await dbContext.Players
            .Include(p => p.User)
            .Where(p => p.RoomId == roomId)
            .ToListAsync();
    }

    public async Task<Player?> GetPlayerInRoomAsync(Guid userId, Guid roomId)
    {
        return await dbContext.Players
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.RoomId == roomId);
    }

    public async Task<bool> PlayerExistsInRoomAsync(Guid userId, Guid roomId)
    {
        return await dbContext.Players
            .AnyAsync(p => p.UserId == userId && p.RoomId == roomId);
    }

    public async Task AddAsync(Player player)
    {
        await dbContext.Players.AddAsync(player);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Player player)
    {
        dbContext.Players.Update(player);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(Player player)
    {
        dbContext.Players.Remove(player);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<Player> players)
    {
        dbContext.Players.RemoveRange(players);
        await dbContext.SaveChangesAsync();
    }
}
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.DataBase.Users;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid? id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserWithPlaylistsDtoAsync(Guid userId)
    {
        return await context.Users
            .Include(u => u.Playlists)
            .ThenInclude(p => p.PlaylistTracks)
            .ThenInclude(pt => pt.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}

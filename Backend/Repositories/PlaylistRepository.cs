using Backend.DataBase;
using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PlaylistRepository(AppDbContext context) : IPlaylistRepository
{
    public async Task<Playlist?> GetByGameSessionIdAsync(Guid gameSessionId)
    {
        return await context.Playlists
            .FirstOrDefaultAsync(p => p.GameSessionId == gameSessionId);
    }

    public async Task AddAsync(Playlist playlist)
    {
        await context.Playlists.AddAsync(playlist);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForGameSessionAsync(Guid gameSessionId)
    {
        return await context.Playlists
            .AnyAsync(p => p.GameSessionId == gameSessionId);
    }
}
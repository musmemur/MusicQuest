using Backend.Entities;

namespace Backend.Repositories;

public interface IPlaylistRepository
{
    Task<Playlist?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task AddAsync(Playlist playlist);
    Task<bool> ExistsForGameSessionAsync(Guid gameSessionId);
}
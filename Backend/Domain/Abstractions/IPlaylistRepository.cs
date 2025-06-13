using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IPlaylistRepository
{
    Task<Playlist?> GetByGameSessionIdAsync(Guid gameSessionId);
    Task AddAsync(Playlist playlist);
    Task<bool> ExistsForGameSessionAsync(Guid gameSessionId);
}
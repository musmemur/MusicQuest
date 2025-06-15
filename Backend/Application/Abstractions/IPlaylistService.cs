using Backend.Domain.Enums;

namespace Backend.Application.Abstractions;

public interface IPlaylistService
{
    Task CreateWinnerPlaylistAsync(string gameSessionId, string winnerId, DeezerGenre genre);
}
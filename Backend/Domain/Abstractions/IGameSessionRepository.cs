using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id);
    Task<GameSession?> GetWithRoomAndPlayersAsync(Guid id);
    Task<GameSession?> GetWithQuestionsAsync(Guid id);
    Task<GameSession?> GetWithRoomAndQuestionsAsync(Guid id);
    Task<GameSession?> GetWithRoomPlayersAndQuestionsAsync(Guid id);
    Task AddAsync(GameSession gameSession);
    Task UpdateAsync(GameSession gameSession);
    Task EndGameSessionAsync(Guid gameSessionId);
}

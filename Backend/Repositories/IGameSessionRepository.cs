using Backend.Entities;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public interface IGameSessionRepository
    {
        Task<GameSession?> GetByIdAsync(Guid id);
        Task<GameSession?> GetWithRoomAndPlayersAsync(Guid id);
        Task<GameSession?> GetWithQuestionsAsync(Guid id);
        Task AddAsync(GameSession gameSession);
        Task UpdateAsync(GameSession gameSession);
        Task EndGameSessionAsync(Guid gameSessionId);
    }
}
using Backend.Entities;
using Backend.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class GameSessionRepository(AppDbContext context) : IGameSessionRepository
    {
        public async Task<GameSession?> GetByIdAsync(Guid id)
        {
            return await context.GameSessions.FindAsync(id);
        }

        public async Task<GameSession?> GetWithRoomAndPlayersAsync(Guid id)
        {
            return await context.GameSessions
                .Include(gs => gs.Room)
                .ThenInclude(r => r.Players)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task<GameSession?> GetWithQuestionsAsync(Guid id)
        {
            return await context.GameSessions
                .Include(gs => gs.Questions)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }
        
        public async Task<GameSession?> GetWithRoomAndQuestionsAsync(Guid id)
        {
            return await context.GameSessions
                .Include(gs => gs.Room)
                .Include(gs => gs.Questions)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }
        
        public async Task<GameSession?> GetWithRoomPlayersAndQuestionsAsync(Guid id)
        {
            return await context.GameSessions
                .Include(gs => gs.Room)
                .ThenInclude(r => r.Players)
                .ThenInclude(p => p.User)
                .Include(gs => gs.Questions)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task AddAsync(GameSession gameSession)
        {
            await context.GameSessions.AddAsync(gameSession);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GameSession gameSession)
        {
            context.GameSessions.Update(gameSession);
            await context.SaveChangesAsync();
        }

        public async Task EndGameSessionAsync(Guid gameSessionId)
        {
            var gameSession = await GetWithRoomAndPlayersAsync(gameSessionId);
            
            if (gameSession != null)
            {
                gameSession.Status = "Completed";
                gameSession.Room.IsActive = false;
                await UpdateAsync(gameSession);
            }
        }
    }
}
using Backend.Dto;
using Backend.Entities;

namespace Backend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<UserPlaylistsDto?> GetUserWithPlaylistsDtoAsync(Guid userId);
    }
}
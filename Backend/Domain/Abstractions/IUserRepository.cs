using Backend.Domain.Entities;
using Backend.Domain.Models;

namespace Backend.Domain.Abstractions
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
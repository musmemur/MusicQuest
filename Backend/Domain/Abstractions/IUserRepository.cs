using Backend.Application.Models;
using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid? id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<UserWithPlaylistsDto?> GetUserWithPlaylistsDtoAsync(Guid userId);
}

using Backend.DataBase;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.DataBase.Users
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task AddAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task<UserPlaylistsDto?> GetUserWithPlaylistsDtoAsync(Guid userId)
        {
            return await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserPlaylistsDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UserPhoto = u.UserPhoto,
                    Playlists = u.Playlists.Select(p => new PlaylistDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Tracks = p.PlaylistTracks.Select(pt => new TrackDto
                        {
                            Id = pt.Track.Id,
                            DeezerTrackId = pt.Track.DeezerTrackId,
                            Title = pt.Track.Title,
                            Artist = pt.Track.Artist,
                            PreviewUrl = pt.Track.PreviewUrl,
                            CoverUrl = pt.Track.CoverUrl
                        }).ToList()
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
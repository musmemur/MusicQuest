using Backend.Domain.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Infrastructure.DataBase.PlaylistTracks;

public class PlaylistTrackRepository(AppDbContext context) : IPlaylistTrackRepository
{
    public async Task AddAsync(PlaylistTrack playlistTrack)
    {
        await context.PlaylistTracks.AddAsync(playlistTrack);
        await context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<PlaylistTrack> playlistTracks)
    {
        await context.PlaylistTracks.AddRangeAsync(playlistTracks);
        await context.SaveChangesAsync();
    }
}
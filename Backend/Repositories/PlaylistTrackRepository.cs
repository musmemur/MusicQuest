using Backend.DataBase;
using Backend.Entities;

namespace Backend.Repositories;

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
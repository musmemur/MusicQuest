using Backend.DataBase;
using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task<Track?> GetByDeezerIdAsync(long deezerTrackId)
    {
        return await context.Tracks
            .FirstOrDefaultAsync(t => t.DeezerTrackId == deezerTrackId);
    }

    public async Task AddAsync(Track track)
    {
        await context.Tracks.AddAsync(track);
        await context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Track> tracks)
    {
        await context.Tracks.AddRangeAsync(tracks);
        await context.SaveChangesAsync();
    }
}
using Backend.Entities;

namespace Backend.Repositories;

public interface ITrackRepository
{
    Task<Track?> GetByDeezerIdAsync(long deezerTrackId);
    Task AddAsync(Track track);
    Task AddRangeAsync(IEnumerable<Track> tracks);
}
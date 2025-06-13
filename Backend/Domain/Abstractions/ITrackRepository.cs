using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface ITrackRepository
{
    Task<Track?> GetByDeezerIdAsync(long deezerTrackId);
    Task AddAsync(Track track);
    Task AddRangeAsync(IEnumerable<Track> tracks);
}
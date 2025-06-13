using Backend.Domain.Entities;

namespace Backend.Domain.Abstractions;

public interface IPlaylistTrackRepository
{
    Task AddAsync(PlaylistTrack playlistTrack);
    Task AddRangeAsync(IEnumerable<PlaylistTrack> playlistTracks);
}
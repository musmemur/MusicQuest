using Backend.Entities;

namespace Backend.Repositories;

public interface IPlaylistTrackRepository
{
    Task AddAsync(PlaylistTrack playlistTrack);
    Task AddRangeAsync(IEnumerable<PlaylistTrack> playlistTracks);
}
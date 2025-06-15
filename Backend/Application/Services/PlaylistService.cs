using Backend.Application.Abstractions;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.DataBase;

namespace Backend.Application.Services;

public class PlaylistService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ITrackRepository trackRepository,
    IDeezerApiClient deezerClient,
    AppDbContext dbContext,
    ILogger<IPlaylistService> logger) : IPlaylistService
{
    public async Task CreateWinnerPlaylistAsync(string gameSessionId, string winnerId, DeezerGenre genre)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            if (await playlistRepository.ExistsForGameSessionAsync(Guid.Parse(gameSessionId)))
            {
                return;
            }

            var tracks = await deezerClient.GenerateTracksToPlaylistByGenreAsync(genre);
            var playlist = new Playlist(Guid.Parse(winnerId), $"Playlist of {genre} Music")
            {
                GameSessionId = Guid.Parse(gameSessionId)
            };

            await playlistRepository.AddAsync(playlist);

            var newTracks = new List<Track>();
            var playlistTracks = new List<PlaylistTrack>();
            
            foreach (var deezerTrack in tracks)
            {
                var existingTrack = await trackRepository.GetByDeezerIdAsync(deezerTrack.Id);
                if (existingTrack == null)
                {
                    existingTrack = new Track
                    {
                        Id = Guid.NewGuid(),
                        DeezerTrackId = deezerTrack.Id,
                        Title = deezerTrack.Title,
                        Artist = deezerTrack.Artist.Name,
                        PreviewUrl = deezerTrack.Preview,
                        CoverUrl = deezerTrack.Album.Cover
                    };
                    newTracks.Add(existingTrack);
                }
                
                playlistTracks.Add(new PlaylistTrack
                {
                    PlaylistId = playlist.Id,
                    TrackId = existingTrack.Id
                });
            }

            if (newTracks.Count > 0)
            {
                await trackRepository.AddRangeAsync(newTracks);
            }

            await playlistTrackRepository.AddRangeAsync(playlistTracks);

            await transaction.CommitAsync();
            logger.LogInformation("Created playlist for winner {WinnerId}", winnerId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
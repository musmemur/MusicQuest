using Backend.DataBase;
using Backend.Entities;
using Backend.Modals;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
public class PlaylistController(AppDbContext dbContext, DeezerApiClient deezerClient, UserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlaylist([FromBody] PlaylistRequest request)
    {
        var userId = userService.GetUserId();
        if (userId == null) return Unauthorized();

        var playlist = new Playlist(userId.Value, request.Title);

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            playlist.Id,
            playlist.Title,
            TracksCount = playlist.PlaylistTracks.Count
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlaylist(Guid id)
    {
        var playlist = await dbContext.Playlists
            .Include(p => p.PlaylistTracks)  // Включаем связующие элементы
            .ThenInclude(pt => pt.Track) // Включаем сами треки
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null) return NotFound();

        return Ok(new
        {
            playlist.Id,
            playlist.Title,
            Tracks = playlist.PlaylistTracks.Select(pt => new
            {
                pt.Track.Id,
                pt.Track.DeezerTrackId,
                pt.Track.Title,
                pt.Track.Artist,
                pt.Track.PreviewUrl,
                pt.Track.CoverUrl
            })
        });
    }
}
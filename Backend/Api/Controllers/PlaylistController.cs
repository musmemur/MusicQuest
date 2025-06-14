using Backend.Infrastructure.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
public class PlaylistController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlaylist(Guid id)
    {
        var playlist = await dbContext.Playlists
            .Include(p => p.PlaylistTracks)
            .ThenInclude(pt => pt.Track)
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
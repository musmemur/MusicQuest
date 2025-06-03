using Backend.Modals;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[ApiController]
[Route("api/quiz")]
public class QuizController(IHubContext<QuizHub> hubContext, DeezerApiClient deezerClient)
    : ControllerBase
{
    private readonly DeezerApiClient _deezerClient = deezerClient;

    [HttpPost("create-room")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var roomId = Guid.NewGuid().ToString();
        await hubContext.Clients.All.SendAsync("RoomCreated", roomId);
        return Ok(new { RoomId = roomId });
    }
}
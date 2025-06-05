using System.Security.Claims;
using Backend.DataBase;
using Backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(AppDbContext dbContext, IHubContext<QuizHub> hubContext) : ControllerBase
{
    // GET: api/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
    {
        var rooms = await dbContext.Rooms
            .Where(r => r.IsActive)
            .Select(r => new RoomDto
            {
                Id = r.Id.ToString(),
                Name = r.Name,
                Genre = r.Genre,
                PlayersCount = r.Players.Count
            })
            .ToListAsync();

        return Ok(rooms);
    }

    // POST: api/rooms
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = $"Комната {roomId.ToString()}",
            Genre = dto.Genre,
            HostUserId = Guid.Parse(userId),
            IsActive = true,
            QuestionsCount = dto.QuestionCount,
        };
    
        var player = new Player
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            RoomId = room.Id,
            Score = 0,
        };

        dbContext.Rooms.Add(room);
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();
    
        return Ok(new { roomId = room.Id.ToString() });
    }
}

public class RoomDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Genre { get; set; }
    public int PlayersCount { get; set; }
}

public class CreateRoomDto
{
    public string Genre { get; set; }
    public int QuestionCount { get; set; }
    public Guid UserHostId { get; set; }
}
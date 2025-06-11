using System.Security.Claims;
using Backend.DataBase;
using Backend.Dto;
using Backend.Entities;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(AppDbContext dbContext, IRoomRepository roomRepository) : ControllerBase
{
    // GET: api/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
    {
        var rooms = await roomRepository.GetActiveRoomsAsync();
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

        await roomRepository.AddAsync(room);
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();
    
        return Ok(new { roomId = room.Id.ToString() });
    }
}
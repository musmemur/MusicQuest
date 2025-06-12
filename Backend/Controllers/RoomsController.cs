using System.Security.Claims;
using Backend.Dto;
using Backend.Entities;
using Backend.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomRepository roomRepository, 
    IPlayerRepository playerRepository, IValidator<CreateRoomDto> createRoomValidator) : ControllerBase
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
        var validationResult = await createRoomValidator.ValidateAsync(dto);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        Enum.TryParse<DeezerGenre>(dto.Genre, true, out var genre);

        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = $"Комната {roomId.ToString()}",
            Genre = genre,
            HostUserId = Guid.Parse(userId),
            IsActive = true,
            QuestionsCount = dto.QuestionCount
        };
    
        var player = new Player
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            RoomId = room.Id,
            Score = 0,
        };

        await roomRepository.AddAsync(room);
        await playerRepository.AddAsync(player);
    
        return Ok(new { roomId = room.Id.ToString() });
    }
}
using System.Security.Claims;
using Backend.Dto;
using Backend.Entities;
using Backend.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(
    IRoomRepository roomRepository, 
    IPlayerRepository playerRepository, 
    IValidator<CreateRoomDto> createRoomValidator,
    ILogger<RoomsController> logger,
    IHubContext<QuizHub> hubContext) : ControllerBase
{
    // GET: api/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
    {
        logger.LogInformation("Запрос списка активных комнат");
        
        try
        {
            var rooms = await roomRepository.GetActiveRoomsAsync();
            logger.LogDebug("Найдено {RoomCount} активных комнат", rooms.Count());
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка комнат");
            return StatusCode(500, "Произошла ошибка при получении комнат");
        }
    }

    // POST: api/rooms
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        logger.LogInformation("Начало создания комнаты. Жанр: {Genre}, Вопросов: {QuestionCount}", 
            dto.Genre, dto.QuestionCount);
        
        var validationResult = await createRoomValidator.ValidateAsync(dto);
        
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Ошибка валидации при создании комнаты: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Попытка создания комнаты без авторизации");
            return Unauthorized();
        }
        
        if (!Enum.TryParse<DeezerGenre>(dto.Genre, true, out var genre))
        {
            logger.LogWarning("Указан недопустимый жанр: {Genre}", dto.Genre);
            return BadRequest("Неправильный жанр");
        }

        var roomId = Guid.NewGuid();
        logger.LogDebug("Создание комнаты с ID: {RoomId}", roomId);
        
        var room = new Room
        {
            Id = roomId,
            Name = $"Комната {roomId}",
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

        try
        {
            await roomRepository.AddAsync(room);
            await playerRepository.AddAsync(player);
            
            logger.LogInformation("Комната успешно создана. ID: {RoomId}, Хост: {UserId}", 
                roomId, userId);
            
            // Отправляем уведомление о новой комнате всем клиентам
            await hubContext.Clients.All.SendAsync("RoomCreated", new RoomDto
            {
                Id = room.Id.ToString(),
                Name = room.Name,
                Genre = room.Genre.ToString(),
                PlayersCount = room.Players.Count
            });
            
            return Ok(new { roomId = room.Id.ToString() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании комнаты");
            return StatusCode(500, "Произошла ошибка при создании комнаты");
        }
    }
}
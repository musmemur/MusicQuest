using System.Security.Claims;
using AutoMapper;
using Backend.Api.Hubs;
using Backend.Api.Models;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(
    IRoomRepository roomRepository, 
    IPlayerRepository playerRepository, 
    IValidator<CreateRoomRequest> createRoomValidator,
    ILogger<RoomsController> logger,
    IHubContext<QuizHub> hubContext, IDeezerApiClient deezerApiClient,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
    {
        logger.LogInformation("Запрос списка активных комнат");
        
        try
        {
            var rooms = await roomRepository.GetActiveRoomsAsync();
            var roomDtos = mapper.Map<IEnumerable<RoomDto>>(
                rooms, 
                opt => opt.Items["DeezerApiClient"] = deezerApiClient
            );
            
            logger.LogDebug("Найдено {RoomCount} активных комнат", rooms.Count());
            return Ok(roomDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка комнат");
            return StatusCode(500, "Произошла ошибка при получении комнат");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        logger.LogInformation("Начало создания комнаты. Жанр: {Genre}, Вопросов: {QuestionCount}", 
            request.Genre, request.QuestionCount);
        
        var validationResult = await createRoomValidator.ValidateAsync(request);
        
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
        
        if (!Enum.TryParse<DeezerGenre>(request.Genre, true, out var genre))
        {
            logger.LogWarning("Указан недопустимый жанр: {Genre}", request.Genre);
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
            QuestionsCount = request.QuestionCount
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
using AutoMapper;
using Backend.Api.Models;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    IUserRepository userRepository, 
    JwtService jwtService, 
    IPasswordHasher<User> passwordHasher, 
    IPhotoSaver imageSaver,
    IValidator<CreateUserRequest> createUserValidator, 
    ILogger<UserService> logger, IUserService userService, IMapper mapper) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        logger.LogInformation("Начало регистрации пользователя {Username}", request.Username);
        
        var validationResult = await createUserValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Ошибка валидации при регистрации: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        if (await userRepository.UsernameExistsAsync(request.Username))
        {
            logger.LogWarning("Попытка регистрации существующего пользователя: {Username}", request.Username);
            return BadRequest("Пользователь с таким логином уже существует");
        }

        if (request.UserPhoto?.Data is not null)
        {
            try
            {
                logger.LogDebug("Обработка фото пользователя");
                var base64Data = request.UserPhoto.Data.Split(',')[1];
                var mimeType = request.UserPhoto.Data.Split(';')[0].Split(':')[1];

                var imageBytes = Convert.FromBase64String(base64Data);
                logger.LogDebug("Сохранение фото в S3");
                var photoPath = await imageSaver.SavePhotoToS3(imageBytes, mimeType, "soundquestphotos");
                request.UserPhoto.FileName = photoPath;
                request.UserPhoto.Data = null;
            }
            catch (FormatException ex)
            {
                logger.LogError(ex, "Ошибка обработки base64 изображения");
                return BadRequest($"Некорректные base64 данные. Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при сохранении изображения");
                return StatusCode(500, "Ошибка при сохранении фото");
            }
        }
        
        var hashedPassword = passwordHasher.HashPassword(null, request.Password);
        logger.LogDebug("Пароль хеширован");
        
        var user = new User(request.Username, hashedPassword, request.UserPhoto?.FileName);
        await userRepository.AddAsync(user);
        logger.LogInformation("Пользователь {Username} успешно зарегистрирован с ID {UserId}", 
            user.Username, user.Id);

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);
        logger.LogDebug("JWT токен сгенерирован для пользователя {UserId}", user.Id);
    
        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        logger.LogInformation("Попытка входа пользователя {Username}", request.Username);
        
        var userInfo = await userRepository.GetByUsernameAsync(request.Username);
        
        if (userInfo == null)
        {
            logger.LogWarning("Попытка входа несуществующего пользователя {Username}", request.Username);
            return Unauthorized("Пользователя с таким логином не существует");
        }
        
        var verificationResult = passwordHasher.VerifyHashedPassword(
            user: null, 
            hashedPassword: userInfo.Password,
            providedPassword: request.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Неверный пароль для пользователя {Username}", request.Username);
            return Unauthorized("Неверный пароль");
        }

        logger.LogDebug("Пароль верифицирован для пользователя {Username}", request.Username);
        var token = await jwtService.GenerateJwtTokenAsync(userInfo.Id, ct);
        logger.LogInformation("Успешный вход пользователя {Username} с ID {UserId}", 
            userInfo.Username, userInfo.Id);
            
        return Ok(new AuthResult(token, userInfo.Id, userInfo.Username, userInfo.UserPhoto));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = userService.GetUserId();

        if (userId == null)
        {
            logger.LogWarning("Попытка доступа к /me без авторизации");
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        logger.LogDebug("Запрос информации о пользователе {UserId}", userId);

        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            logger.LogWarning("Пользователь {UserId} не найден в базе", userId);
            return Unauthorized(new { message = "Пользователь не найден" });
        }

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);
        logger.LogDebug("Обновление токена для пользователя {UserId}", user.Id);

        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }
    
    [HttpGet("get-user-with-playlists/{userId:guid}")]
    public async Task<IActionResult> GetUserWithPlaylists(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Запрос информации о пользователе с плейлистами {UserId}", userId);
        
        var user = await userRepository.GetUserWithPlaylistsDtoAsync(userId);
    
        if (user == null)
        {
            logger.LogWarning("Пользователь с плейлистами {UserId} не найден", userId);
            return NotFound();
        }
        
        var userInfo = mapper.Map<UserWithPlaylistsDto>(user);
        
        logger.LogDebug("Найдено {Count} плейлистов для пользователя {UserId}", 
            userInfo.Playlists.Count, userId);
    
        return Ok(userInfo);
    }
}
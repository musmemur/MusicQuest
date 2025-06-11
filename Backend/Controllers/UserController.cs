using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.DataBase;
using Backend.Entities;
using Backend.Modals;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserRepository userRepository, JwtService jwtService, 
    IPasswordHasher<User> passwordHasher, ImageSaver imageSaver) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (await userRepository.UsernameExistsAsync(request.Username))
        {
            return BadRequest("Пользователь с таким логином уже существует");
        }

        if (request.UserPhoto?.Data is not null)
        {
            try
            {
                var base64Data = request.UserPhoto.Data.Split(',')[1];
                var mimeType = request.UserPhoto.Data.Split(';')[0].Split(':')[1];

                var imageBytes = Convert.FromBase64String(base64Data);

                var photoPath = await imageSaver.SaveImageToS3(imageBytes, mimeType, "soundquestphotos");
                request.UserPhoto.FileName = photoPath;
                request.UserPhoto.Data = null;
            }
            catch (FormatException ex)
            {
                return BadRequest($"Некорректные base64 данные. Ошибка: {ex.Message}");
            }
        }
        
        var hashedPassword = passwordHasher.HashPassword(null, request.Password);
        
        var user = new User(
            request.Username, 
            hashedPassword, 
            request.UserPhoto?.FileName);

        await userRepository.AddAsync(user);

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);
    
        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var userInfo = await userRepository.GetByUsernameAsync(request.Username);
        
        if (userInfo == null)
        {
            return Unauthorized("Пользователя с таким логином не существует");
        }
        
        var verificationResult = passwordHasher.VerifyHashedPassword(
            user: null, 
            hashedPassword: userInfo.Password,
            providedPassword: request.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Неверный пароль");
        }

        var token = await jwtService.GenerateJwtTokenAsync(userInfo.Id, ct);
        return Ok( new AuthResult(token, userInfo.Id, userInfo.Username, userInfo.UserPhoto));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        var guid = Guid.Parse(userId);

        var user = await userRepository.GetByIdAsync(guid);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);

        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }
    
    [HttpGet("get-user-with-playlists/{userId:guid}")]
    public async Task<IActionResult> GetUserWithPlaylists(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var userInfo = await userRepository.GetUserWithPlaylistsDtoAsync(userId);
    
        if (userInfo == null)
        {
            return NotFound();
        }
    
        return Ok(userInfo);
    }
}
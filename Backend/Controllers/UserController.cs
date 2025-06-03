using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.DataBase;
using Backend.Entities;
using Backend.Modals;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(AppDbContext dbContext, JwtService jwtService, 
    IPasswordHasher<User> passwordHasher, UserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username, ct))
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

                var photoPath = await ImageSaver.SaveImageToS3(imageBytes, mimeType, "soundquestphotos");
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

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct); 

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);
    
        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var userInfo = await dbContext.Users
            .Where(u => u.Username == request.Username)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
        
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

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == guid, ct);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        var token = await jwtService.GenerateJwtTokenAsync(user.Id, ct);

        return Ok(new AuthResult(token, user.Id, user.Username, user.UserPhoto));
    }
    
    [HttpGet("get-user-with-playlists/{userId}")]
    public async Task<IActionResult> GetUserWithPlaylists(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var userInfo = await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Playlists)
            .ThenInclude(p => p.PlaylistTracks)
            .ThenInclude(pt => pt.Track)
            .AsNoTracking()
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.UserPhoto,
                Playlists = u.Playlists.Select(p => new 
                {
                    p.Id,
                    p.Title,
                    PlaylistTracks = p.PlaylistTracks.Select(pt => new 
                    {
                        Track = new 
                        {
                            pt.Track.Id,
                            pt.Track.DeezerTrackId,
                            pt.Track.Title,
                            pt.Track.Artist,
                            pt.Track.PreviewUrl,
                            pt.Track.CoverUrl
                        }
                    })
                })
            })
            .FirstOrDefaultAsync(cancellationToken);
    
        if (userInfo == null)
        {
            return NotFound();
        }
    
        return Ok(userInfo);
    }
}
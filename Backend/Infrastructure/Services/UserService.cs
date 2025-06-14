using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Domain.Abstractions;

namespace Backend.Infrastructure.Services;

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Guid? GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        var userIdString = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }
    
}
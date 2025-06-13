using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Domain.Services;

public class UserService(IHttpContextAccessor httpContextAccessor)
{
    public Guid? GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        var userIdString = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }
    
}
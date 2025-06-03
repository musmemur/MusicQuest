using System.IdentityModel.Tokens.Jwt;

namespace Backend.Services;

using System.Security.Claims;

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
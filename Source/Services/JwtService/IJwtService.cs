using System.Security.Claims;

namespace Source.Services.JwtService;

public interface IJwtService
{
    string GenerateToken(Dictionary<string, string> claims, int? expireMinutes = null);

    ClaimsPrincipal? ValidateToken(string token);
}

using System.Security.Claims;

namespace Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(Dictionary<string, string> claims, int? expireMinutes = null);

        ClaimsPrincipal? ValidateToken(string token);
    }
}

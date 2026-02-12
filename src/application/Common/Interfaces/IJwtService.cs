using System.Security.Claims;

namespace Api.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Dictionary<string, string> claims, int? expireMinutes = null);

    ClaimsPrincipal? ValidateToken(string token);
}

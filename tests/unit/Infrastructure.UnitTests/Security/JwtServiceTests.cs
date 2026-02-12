using Api.Infrastructure.Config;
using Api.Infrastructure.Security.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Infrastructure.UnitTests.Security;

public class JwtServiceTests
{
    private static JwtService CreateService(string secret = "super-strong-test-secret-key-123456")
    {
        var settings = new JwtSettings
        {
            Secret = secret,
            ExpiryMinutes = 30
        };

        return new JwtService(Options.Create(settings));
    }

    [Fact]
    public void GenerateToken_ShouldEncodeClaims()
    {
        var service = CreateService();
        var payload = new Dictionary<string, string>
        {
            [JwtRegisteredClaimNames.Sub] = "user-123",
            [JwtRegisteredClaimNames.Email] = "user@test.local"
        };

        string token = service.GenerateToken(payload);
        var principal = service.ValidateToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be("user-123");
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.Should().Be("user@test.local");
    }

    [Fact]
    public void GenerateToken_ShouldRespectCustomExpiry()
    {
        var service = CreateService();
        var payload = new Dictionary<string, string> { ["role"] = "admin" };
        DateTime expectedUpperBound = DateTime.UtcNow.AddMinutes(5);

        string token = service.GenerateToken(payload, expireMinutes: 5);
        JwtSecurityToken parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsedToken.ValidTo.Should().BeCloseTo(expectedUpperBound, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ValidateToken_ShouldReturnNullForInvalidSignature()
    {
        var service = CreateService();
        var payload = new Dictionary<string, string> { ["scope"] = "read" };

        string token = service.GenerateToken(payload);
        var tamperedToken = token.Replace('a', 'b');

        var principal = service.ValidateToken(tamperedToken);

        principal.Should().BeNull();
    }
}

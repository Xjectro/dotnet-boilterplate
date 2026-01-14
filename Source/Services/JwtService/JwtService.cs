using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Source.Configurations;

namespace Source.Services.JwtService;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateToken(Dictionary<string, string> payload, int? expireMinutes = null)
    {
        byte[] key = Encoding.ASCII.GetBytes(_settings.SECRET);
        DateTime expires = DateTime.UtcNow.AddMinutes(expireMinutes ?? _settings.EXPIRY_MINUTES);

        var claims = payload.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            // Issuer = _settings.Issuer,
            // Audience = _settings.Audience
        };

        SecurityToken token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        byte[] key = Encoding.ASCII.GetBytes(_settings.SECRET);

        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            ClaimsPrincipal principal = _tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}

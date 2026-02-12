using Api.Application.Common.Interfaces;
using BCryptNet = global::BCrypt.Net.BCrypt;

namespace Api.Infrastructure.Security.BCrypt;

public class BCryptService : IBCryptService
{
    public string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password, 12);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return BCryptNet.Verify(providedPassword, hashedPassword);
    }
}

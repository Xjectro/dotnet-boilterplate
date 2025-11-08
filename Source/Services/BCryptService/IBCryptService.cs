using Source.Models;

namespace Source.Services;

public interface IBCryptService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

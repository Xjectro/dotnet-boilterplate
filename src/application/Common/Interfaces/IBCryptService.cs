namespace Api.Application.Common.Interfaces;

public interface IBCryptService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

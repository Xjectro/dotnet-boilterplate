namespace Source.Services.BCryptService;

public interface IBCryptService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

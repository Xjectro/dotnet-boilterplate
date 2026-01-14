namespace Source.Configurations;

public class JwtSettings
{
    public string SECRET { get; set; } = string.Empty;
    public int EXPIRY_MINUTES { get; set; } = 60;
}

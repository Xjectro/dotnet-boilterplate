namespace Source.Configurations;

public class RateLimitSettings
{
    public required int PermitLimit { get; set; }
    public required int WindowSeconds { get; set; }
    public required int QueueLimit { get; set; }
}

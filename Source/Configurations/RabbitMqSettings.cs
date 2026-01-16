namespace Source.Configurations;

public class RabbitMqSettings
{
    public required string HOST { get; set; }
    public required int PORT { get; set; }
    public required string USERNAME { get; set; }
    public required string PASSWORD { get; set; }
    public required string VIRTUAL_HOST { get; set; }
}

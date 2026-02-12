namespace Api.Infrastructure.Config;

public class MailSettings
{
    public required string SmtpHost { get; set; }
    public required int SmtpPort { get; set; }
    public required string SmtpUsername { get; set; }
    public required string SmtpPassword { get; set; }
    public required string FromEmail { get; set; }
    public required string FromName { get; set; }
    public required bool EnableSsl { get; set; }
    public required string MailQueueName { get; set; }
}

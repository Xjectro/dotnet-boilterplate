namespace Api.Application.Common.Interfaces;

public interface IMailService
{
    /// <summary>
    /// Queues a message to RabbitMQ for sending email
    /// </summary>
    Task QueueEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Queues a message to RabbitMQ for sending email (multiple recipients)
    /// </summary>
    Task QueueEmailAsync(string[] to, string subject, string body, bool isHtml = true);
}

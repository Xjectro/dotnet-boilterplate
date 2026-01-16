using Microsoft.Extensions.Options;
using Source.Configurations;
using Source.Services.RabbitMqService;
using Source.Services.WorkerService;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Source.Services.MailService;

public class MailService : IMailService, IWorkerService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly MailSettings _mailSettings;
    private readonly ILogger<MailService> _logger;

    public MailService(
        IRabbitMqService rabbitMqService,
        IOptions<MailSettings> mailSettings,
        ILogger<MailService> logger)
    {
        _rabbitMqService = rabbitMqService;
        _mailSettings = mailSettings.Value;
        _logger = logger;
        InitializeQueue();
    }

    public string QueueName => _mailSettings.MailQueueName;

    private void InitializeQueue()
    {
        try
        {
            _rabbitMqService.DeclareQueue(_mailSettings.MailQueueName, durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation("Mail queue '{QueueName}' initialized successfully.", _mailSettings.MailQueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize mail queue '{QueueName}'.", _mailSettings.MailQueueName);
        }
    }

    public async Task QueueEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        await QueueEmailAsync(new[] { to }, subject, body, isHtml);
    }

    public async Task QueueEmailAsync(string[] to, string subject, string body, bool isHtml = true)
    {
        var emailMessage = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml,
            From = _mailSettings.FromEmail,
            FromName = _mailSettings.FromName,
            QueuedAt = DateTime.UtcNow
        };

        await QueueMessageAsync(emailMessage);
    }

    private async Task QueueMessageAsync(EmailMessage emailMessage)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(emailMessage);
            _rabbitMqService.PublishMessage(_mailSettings.MailQueueName, messageJson);
            
            _logger.LogInformation(
                "Email queued successfully. To: {Recipients}, Subject: {Subject}",
                string.Join(", ", emailMessage.To),
                emailMessage.Subject);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email. Subject: {Subject}", emailMessage.Subject);
            throw;
        }
    }

    public async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);
        if (emailMessage == null)
        {
            _logger.LogWarning("Failed to deserialize email message");
            return;
        }

        await SendEmailAsync(emailMessage, cancellationToken);
        _logger.LogInformation("Email sent successfully to: {Recipients}", string.Join(", ", emailMessage.To));
    }

    private async Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient(_mailSettings.SmtpHost, _mailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_mailSettings.SmtpUsername, _mailSettings.SmtpPassword),
            EnableSsl = _mailSettings.EnableSsl
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(emailMessage.From, emailMessage.FromName),
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsBodyHtml = emailMessage.IsHtml
        };

        foreach (var to in emailMessage.To)
        {
            mailMessage.To.Add(to);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}

public class EmailMessage
{
    public required string[] To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required bool IsHtml { get; set; }
    public required string From { get; set; }
    public required string FromName { get; set; }
    public DateTime QueuedAt { get; set; }
}

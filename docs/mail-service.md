# Mail Service Documentation

## Overview
The Mail Service provides asynchronous email sending with RabbitMQ integration.

## Architecture

```
API Request
    ↓
MailController
    ↓
MailService.QueueEmailAsync() → RabbitMQ Queue
    ↓
WorkerService (Background) → MailService.ProcessMessageAsync()
    ↓
SMTP → Email Sent
```

## Configuration

### appsettings.json
```json
{
  "Mail": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@yourapp.com",
    "FromName": "Your App Name",
    "EnableSsl": true,
    "MailQueueName": "mail_queue"
  }
}
```

### Environment Variables (Docker)
```env
Mail__SmtpHost=smtp.gmail.com
Mail__SmtpPort=587
Mail__SmtpUsername=your-email@gmail.com
Mail__SmtpPassword=your-app-password
Mail__FromEmail=noreply@yourapp.com
Mail__FromName=Your App Name
Mail__EnableSsl=true
Mail__MailQueueName=mail_queue
```

## Service Implementation

### MailService.cs
Location: `src/infrastructure/Mail/MailService.cs`

**Implements Two Interfaces:**
- `IMailService` - Queues emails
- `IWorkerService` - Processes queued emails

### Interface Methods

```csharp
public interface IMailService
{
    Task QueueEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task QueueEmailAsync(string[] to, string subject, string body, bool isHtml = true);
}
```

## API Endpoints

### Send Email
**POST** `/api/mail/send`

**Request Body:**
```json
{
  "to": ["user@example.com", "admin@example.com"],
  "subject": "Welcome!",
  "body": "<h1>Hello World</h1>",
  "isHtml": true
}
```

**Response:**
```json
{
  "message": "Email successfully queued",
  "success": true
}
```

## Usage Examples

### 1. Basic Email (Single Recipient)
```csharp
await _mailService.QueueEmailAsync(
    to: "user@example.com",
    subject: "Welcome!",
    body: "<h1>Welcome to our platform</h1>",
    isHtml: true
);
```

### 2. Multiple Recipients
```csharp
await _mailService.QueueEmailAsync(
    to: new[] { "user1@example.com", "user2@example.com" },
    subject: "Newsletter",
    body: "Latest updates...",
    isHtml: false
);
```

### 3. From Controller
```csharp
[HttpPost("send")]
public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
{
    await _mailService.QueueEmailAsync(
        request.To,
        request.Subject,
        request.Body,
        request.IsHtml
    );
    
    return Ok(new { message = "Email successfully queued", success = true });
}
```

## How It Works

### Step 1: Queueing (API Call)
1. API receives email request
2. `MailService.QueueEmailAsync()` is called
3. Email data is serialized to JSON
4. Message is published to RabbitMQ queue
5. API returns immediately (email NOT sent yet)

### Step 2: Processing (Background)
1. `WorkerService` continuously listens to mail queue
2. When message arrives, `ProcessMessageAsync()` is called
3. Message is deserialized to `EmailMessage`
4. `SendEmailAsync()` connects to SMTP and sends email
5. On success: Message removed from queue (`BasicAck`)
6. On failure: Message requeued for retry (`BasicNack`)

## Message Structure

### EmailMessage Class
```csharp
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
```

## SMTP Configuration

### Gmail Example
1. Enable 2FA on your Google account
2. Generate an App Password
3. Use App Password in configuration

```json
{
  "Mail": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-16-char-app-password",
    "EnableSsl": true
  }
}
```

### Other SMTP Providers

**Outlook/Office365:**
```json
{
  "SmtpHost": "smtp.office365.com",
  "SmtpPort": 587
}
```

**SendGrid:**
```json
{
  "SmtpHost": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "SmtpUsername": "apikey",
  "SmtpPassword": "your-sendgrid-api-key"
}
```

## Error Handling

### Queue Errors
```csharp
try
{
    await _mailService.QueueEmailAsync(to, subject, body);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to queue email");
    // Email was not queued
}
```

### SMTP Errors
Handled automatically by WorkerService:
- Connection errors → Message requeued
- Authentication errors → Logged and requeued
- Send failures → Requeued with exponential backoff

## Monitoring

### Check Queue Status
1. Open RabbitMQ Management UI: http://localhost:15672
2. Navigate to Queues
3. Find `mail_queue`
4. View:
   - Ready messages (waiting to be processed)
   - Unacked messages (being processed)
   - Total messages

### Application Logs
```
[INFO] Email queued successfully. To: user@example.com, Subject: Welcome
[INFO] Message processed successfully from queue: mail_queue
[INFO] Email sent successfully to: user@example.com
```

## Best Practices

1. **Always Use Queue**: Never send emails synchronously from API
2. **HTML Emails**: Use proper HTML templates
3. **Test SMTP**: Verify credentials before deployment
4. **Monitor Queue**: Check for stuck messages
5. **Rate Limiting**: Be aware of SMTP provider limits
6. **Retry Logic**: Failed emails automatically retry
7. **Logging**: Monitor both queueing and sending logs

## Troubleshooting

### Emails Not Sending
1. Check RabbitMQ queue has messages
2. Verify SMTP credentials
3. Check WorkerService is running
4. Review application logs
5. Test SMTP connection manually

### Queue Growing
- SMTP credentials might be wrong
- SMTP server might be down
- Check application logs for errors
- Temporarily pause queueing if needed

### Duplicate Emails
- Usually happens on retry after failure
- Check for multiple worker instances
- Verify message acknowledgment logic

## Performance Considerations

- **Async Operations**: All methods are async
- **Background Processing**: API responds immediately
- **Scalable**: Can run multiple worker instances
- **Reliable**: Messages survive application restarts
- **Throttling**: Controlled by RabbitMQ prefetch settings

# RabbitMQ Message Queue Usage

## Overview
RabbitMQ is used for asynchronous task processing and message queuing. It is especially used for operations like email sending.

## Configuration

### appsettings.json
```json
{
    "RabbitMq": {
        "Host": "rabbitmq",
        "Port": 5672,
        "Username": "admin",
        "Password": "admin123",
        "VirtualHost": "/"
    }
}
```

### Environment Variables (Docker)
```env
RabbitMq__Host=rabbitmq
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=admin123
RabbitMq__VirtualHost=/
```

## Service Implementation

### RabbitMqService.cs
Location: `src/infrastructure/Messaging/RabbitMq/RabbitMqService.cs`

**Key Features:**
- Connection pooling and management
- Automatic reconnection
- Queue declaration
- Message publishing
- Exchange management

### Interface Methods
```csharp
public interface IRabbitMqService
{
        IConnection GetConnection();
        IModel CreateChannel();
        void PublishMessage(string queueName, string message);
        void PublishMessage(string exchangeName, string routingKey, string message);
        void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
        void DeclareExchange(string exchangeName, string type = ExchangeType.Direct, bool durable = true, bool autoDelete = false);
        void BindQueue(string queueName, string exchangeName, string routingKey);
}
```

## Usage Examples

### 1. Publishing Messages
```csharp
public class MailService
{
        private readonly IRabbitMqService _rabbitMqService;
        public async Task QueueEmailAsync(string to, string subject, string body)
        {
                var emailMessage = new EmailMessage { To = to, Subject = subject, Body = body };
                var messageJson = JsonSerializer.Serialize(emailMessage);
                // Publish to queue
                _rabbitMqService.PublishMessage("mail_queue", messageJson);
        }
}
```

### 2. Declaring Queues
```csharp
// Durable queue (survives broker restart)
_rabbitMqService.DeclareQueue("mail_queue", durable: true, exclusive: false, autoDelete: false);
```

### 3. Working with Exchanges
```csharp
// Declare exchange
_rabbitMqService.DeclareExchange("email_exchange", ExchangeType.Topic, durable: true);

// Bind queue to exchange
_rabbitMqService.BindQueue("mail_queue", "email_exchange", "email.send.*");

// Publish to exchange
_rabbitMqService.PublishMessage("email_exchange", "email.send.notification", messageJson);
```

## Worker Service Integration

### How It Works

1. WorkerService starts when the application launches
2. Discovers all `IWorkerService` implementations
3. Creates a RabbitMQ consumer for each worker's queue
4. Continuously listens for messages
5. Processes messages and acknowledges/rejects them

### Worker Service Flow

```
Application Start
    ↓
WorkerService.ExecuteAsync()
    ↓
Find all IWorkerService implementations (e.g., MailService)
    ↓
For each worker:
    - Create RabbitMQ channel
    - Declare queue
    - Set up consumer
    - Listen for messages
    ↓
Message Received
    ↓
Call worker.ProcessMessageAsync(message)
    ↓
Success: BasicAck (remove from queue)
Failure: BasicNack (requeue message)
```

### Example Worker Implementation

```csharp
public class MailService : IMailService, IWorkerService
{
    public string QueueName => "mail_queue";
    
    public async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        // Deserialize message
        var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);
        
        // Process (send email)
        await SendEmailAsync(emailMessage, cancellationToken);
    }
}
```

## Message Flow Example (Email Service)

```
1. API Request: POST /api/mail/send
   ↓
2. MailService.QueueEmailAsync()
   ↓
3. Serialize EmailMessage to JSON
   ↓
4. RabbitMqService.PublishMessage("mail_queue", json)
   ↓
5. Message added to RabbitMQ queue
   ↓
6. API returns success (mail not sent yet!)
   ↓
[Background Processing]
   ↓
7. WorkerService listening on "mail_queue"
   ↓
8. Message received
   ↓
9. MailService.ProcessMessageAsync() called
   ↓
10. Send email via SMTP
   ↓
11a. Success → BasicAck → Message removed from queue
11b. Failure → BasicNack → Message requeued
```

## Queue Properties

### Durable Queues
- Survive RabbitMQ restarts
- Messages are persisted to disk
- Used for critical operations (like emails)

```csharp
_rabbitMqService.DeclareQueue("mail_queue", durable: true);
```

### Message Properties
- **Persistent**: Messages survive broker restart
- **Priority**: Message priority levels
- **TTL**: Time-to-live for messages
- **Dead Letter**: Failed message routing

## Management UI

Access RabbitMQ management interface:
- **URL**: http://localhost:15672
- **Username**: admin
- **Password**: admin123

**Features:**
- View queues and messages
- Monitor connections
- Manage exchanges
- View message rates

## Best Practices

1. **Use Durable Queues**: For important operations
2. **Set Persistent Messages**: Ensure messages survive restarts
3. **Handle Failures**: Implement retry logic with BasicNack
4. **Prefetch Count**: Limit concurrent message processing
5. **Connection Management**: Reuse connections, create new channels

## Error Handling

```csharp
try
{
    _rabbitMqService.PublishMessage("mail_queue", message);
}
catch (BrokerUnreachableException ex)
{
    _logger.LogError(ex, "RabbitMQ broker unreachable");
    // Implement fallback or retry logic
}
```

## Docker Integration

RabbitMQ container configuration in `deploy/docker/dev/docker-compose.yml`:
- Management UI enabled
- Default vhost configured
- Data persistence with volumes
- Health checks included

## Monitoring

Check queue status:
1. Open Management UI
2. Navigate to "Queues" tab
3. View message count, rates, and consumers
4. Monitor memory and connection usage

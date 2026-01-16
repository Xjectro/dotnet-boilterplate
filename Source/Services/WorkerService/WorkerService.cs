using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Source.Services.WorkerService;

public class WorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkerService> _logger;
    private readonly List<IModel> _channels = new();

    public WorkerService(
        IServiceProvider serviceProvider,
        ILogger<WorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service is starting...");

        using var scope = _serviceProvider.CreateScope();
        var workers = scope.ServiceProvider.GetServices<IWorkerService>();

        foreach (var worker in workers)
        {
            try
            {
                StartConsumer(worker, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start consumer for queue: {QueueName}", worker.QueueName);
            }
        }

        _logger.LogInformation("Worker Service is running with {WorkerCount} workers", workers.Count());

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void StartConsumer(IWorkerService worker, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var rabbitMqService = scope.ServiceProvider.GetRequiredService<Source.Services.RabbitMqService.IRabbitMqService>();
        
        var channel = rabbitMqService.CreateChannel();
        _channels.Add(channel);

        channel.QueueDeclare(
            queue: worker.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                using var workerScope = _serviceProvider.CreateScope();
                var scopedWorker = workerScope.ServiceProvider.GetServices<IWorkerService>()
                    .FirstOrDefault(w => w.QueueName == worker.QueueName);

                if (scopedWorker != null)
                {
                    await scopedWorker.ProcessMessageAsync(message, stoppingToken);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Message processed successfully from queue: {QueueName}", worker.QueueName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue: {QueueName}", worker.QueueName);
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(queue: worker.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Started consuming from queue: {QueueName}", worker.QueueName);
    }

    public override void Dispose()
    {
        foreach (var channel in _channels)
        {
            channel?.Close();
            channel?.Dispose();
        }
        _channels.Clear();
        base.Dispose();
    }
}

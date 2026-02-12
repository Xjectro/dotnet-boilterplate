using Api.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace Api.Infrastructure.Background;

public class WorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IModel> _channels = new();

    public WorkerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Worker Service is starting...");

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
                Log.Error(ex, "Failed to start consumer for queue: {QueueName}", worker.QueueName);
            }
        }

        Log.Information("Worker Service is running with {WorkerCount} workers", workers.Count());

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void StartConsumer(IWorkerService worker, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

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
                    Log.Information("Message processed successfully from queue: {QueueName}", worker.QueueName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing message from queue: {QueueName}", worker.QueueName);
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(queue: worker.QueueName, autoAck: false, consumer: consumer);
        Log.Information("Started consuming from queue: {QueueName}", worker.QueueName);
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

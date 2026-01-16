namespace Source.Services.WorkerService;

public interface IWorkerService
{
    string QueueName { get; }
    Task ProcessMessageAsync(string message, CancellationToken cancellationToken);
}

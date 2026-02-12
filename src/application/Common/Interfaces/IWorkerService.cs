namespace Api.Application.Common.Interfaces;

public interface IWorkerService
{
    string QueueName { get; }
    Task ProcessMessageAsync(string message, CancellationToken cancellationToken);
}

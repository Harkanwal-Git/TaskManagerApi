using TaskManagerApi.Producers.Events;

namespace TaskManagerApi.Producers;

public interface ITaskProducer
{
    Task PublishTaskCreatedAsync(TaskCreatedEvent taskCreatedEvent, CancellationToken ct);
}
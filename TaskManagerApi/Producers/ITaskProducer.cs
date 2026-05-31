using taskmanager.events;


namespace TaskManagerApi.Producers;

public interface ITaskProducer
{
    Task PublishTaskCreatedAsync(TaskCreatedEvent taskCreatedEvent, CancellationToken ct);
}
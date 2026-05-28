using TaskManagerApi.Events;

namespace TaskManagerApi.EventPublisher;

public interface IEventPublisher
{
    Task PublishAsync(string payload, CancellationToken ct);
}
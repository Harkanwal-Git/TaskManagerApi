using System.Text.Json;
using TaskManagerApi.EventPublisher;
using TaskManagerApi.Events;
using TaskManagerApi.Producers.Events;

namespace TaskManagerApi.Producers.EventPublisher;


public class TaskCreatedEventPublisher : IEventPublisher
{
    private readonly ITaskProducer _taskProducer;
    public TaskCreatedEventPublisher(ITaskProducer taskProducer)
    {
        _taskProducer = taskProducer;
    }
    public async Task PublishAsync(string payload, CancellationToken ct)
    {

        var taskCreatedEvent = JsonSerializer.Deserialize<TaskCreatedEvent>(payload);
        await _taskProducer.PublishTaskCreatedAsync(taskCreatedEvent!, ct);
    }
}
using System.Text.Json;
using taskmanager.events;
using TaskManagerApi.EventPublisher;
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

        var taskCreatedEventDto = JsonSerializer.Deserialize<TaskCreatedEventDto>(payload) ?? throw new JsonException("Null returned on deserialization");
        var taskCreatedEvent = new TaskCreatedEvent()
        {
            taskId = taskCreatedEventDto.TaskId.ToString(),
            userId = taskCreatedEventDto.UserId.ToString(),
            title = taskCreatedEventDto.Title,
            description = taskCreatedEventDto.Description,
            occuredAt = taskCreatedEventDto.OccurredAt,
            isCompleted = taskCreatedEventDto.IsCompleted
        };
        await _taskProducer.PublishTaskCreatedAsync(taskCreatedEvent, ct);
    }
}
using System.Text.Json;
using MediatR;
using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Producers;
using TaskManagerApi.Producers.Events;
using TaskManagerApi.Repository;

namespace TaskManagerApi.CQRS.Commands;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, ResponseTaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskProducer _taskProducer;
    public CreateTaskHandler(ITaskRepository taskRepository, ITaskProducer taskProducer)
    {
        _taskRepository = taskRepository;
        _taskProducer = taskProducer;
    }

    public async Task<ResponseTaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.CreateTaskDto.AssignedUserId.HasValue)
        {
            throw new UnauthorizedAccessException("Only admins can assign tasks to other users");
        }
        var taskAssignee = request.CreateTaskDto.AssignedUserId.HasValue && request.IsAdmin ? request.CreateTaskDto.AssignedUserId.Value : request.userId;
        TaskItem task = new TaskItem() { Title = request.CreateTaskDto.Title, Description = request.CreateTaskDto.Description, UserId = taskAssignee };
        var taskCreated = new TaskCreatedEvent
        {
            TaskId = task.Id,
            Title = task.Title,
            Description = task.Description,
            UserId = task.UserId,
            IsCompleted = task.IsCompleted,
            OccurredAt = task.CreatedAt
        };
        var outboxMessage = new OutboxMessage() { Id = Guid.NewGuid(), Payload = JsonSerializer.Serialize(taskCreated), EventType = Events.EventType.TaskCreated };
        var result = await _taskRepository.AddTask(task, outboxMessage, cancellationToken);


        // await _taskProducer.PublishTaskCreatedAsync(new TaskCreatedEvent
        // {
        //     TaskId = result.Id,
        //     Title = result.Title,
        //     Description = result.Description,
        //     UserId = result.UserId,
        //     IsCompleted = result.IsCompleted,
        //     OccurredAt = result.CreatedAt
        // }, cancellationToken);

        return new ResponseTaskDto(Id: result.Id,
        Title: result.Title, Description: result.Description,
         IsCompleted: result.IsCompleted, CreatedAt: result.CreatedAt,
         UserId: result.UserId, Tags: result.TaskTags.Select(tt => new ResponseTagDto(tt.TagId, tt.Tag.TagName)).ToList());
    }
}
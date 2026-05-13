using MediatR;
using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.CQRS.Commands;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, ResponseTaskDto>
{
    private readonly ITaskRepository _taskRepository;
    public CreateTaskHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<ResponseTaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.CreateTaskDto.AssignedUserId.HasValue)
        {
            throw new UnauthorizedAccessException("Only admins can assign tasks to other users");
        }
        var taskAssignee = request.CreateTaskDto.AssignedUserId.HasValue && request.IsAdmin ? request.CreateTaskDto.AssignedUserId.Value : request.userId;
        TaskItem task = new TaskItem() { Title = request.CreateTaskDto.Title, Description = request.CreateTaskDto.Description, UserId = taskAssignee };
        var result = await _taskRepository.AddTask(task, cancellationToken);

        return new ResponseTaskDto(Id: result.Id,
        Title: result.Title, Description: result.Description,
         IsCompleted: result.IsCompleted, CreatedAt: result.CreatedAt,
         UserId: result.UserId, Tags: result.TaskTags.Select(tt => new ResponseTagDto(tt.TagId, tt.Tag.TagName)).ToList());
    }
}
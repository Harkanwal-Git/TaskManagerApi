using MediatR;
using TaskManagerApi.DTO;
using TaskManagerApi.Repository;

namespace TaskManagerApi.CQRS.Queries;


public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, ResponseTaskDto?>
{
    private readonly ITaskRepository _taskRepository;
    public GetTaskByIdHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }
    public async Task<ResponseTaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _taskRepository.GetTaskById(request.TaskId, request.UserId, request.IsAdmin, cancellationToken);

        return result != null ? new ResponseTaskDto(Id: result.Id, Title: result.Title, Description: result.Description, CreatedAt: result.CreatedAt, UserId: result.UserId, IsCompleted: result.IsCompleted,
        Tags: result.TaskTags.Select(tt => new ResponseTagDto(TagId: tt.TagId, TagName: tt.Tag.TagName)).ToList()) : null;


    }
}
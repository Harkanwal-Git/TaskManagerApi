using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    public TaskService(ITaskRepository taskRepository)
    {
        this._taskRepository = taskRepository;
    }
    public async Task<ResponseTaskDto> AddTask(CreateTaskDto createTaskDto, Guid userId, bool isAdmin)
    {
        ResponseTaskDto responseTaskDto;
        var taskAssignee = isAdmin && createTaskDto.AssignedUserId.HasValue ? createTaskDto.AssignedUserId.Value : userId;
        TaskItem taskItem = await _taskRepository.AddTask(MapCreateRequestDtoToTaskItem(createTaskDto, taskAssignee));
        responseTaskDto = MapTaskItemToResponseDto(taskItem);
        return responseTaskDto;
    }

    public async Task<IEnumerable<ResponseTaskDto>> GetAllTasks(Guid userId, bool isAdmin)
    {
        var result = await _taskRepository.GetAllTasks(userId, isAdmin);
        return result.Select(t => MapTaskItemToResponseDto(t));
    }

    public async Task<ResponseTaskDto?> GetTaskById(Guid id, Guid userId, bool isAdmin)
    {
        TaskItem? taskItem = await _taskRepository.GetTaskById(id, userId, isAdmin);

        return taskItem != null ? MapTaskItemToResponseDto(taskItem) : null;
    }

    public async Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin)
    {
        return await _taskRepository.DeleteTask(taskId, userId, isAdmin);
    }
    public async Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId, Guid userId, bool isAdmin)
    {
        var taskItem = MapUpdateTaskDtoToTaskItem(updateTaskDto, taskId, userId);
        var updatedTaskItem = await _taskRepository.UpdateTask(taskItem, isAdmin);

        return updatedTaskItem != null ? MapTaskItemToResponseDto(updatedTaskItem) : null;
    }

    public async Task<IEnumerable<ResponseTaskDto>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin)
    {
        IEnumerable<TaskItem> tasks = await _taskRepository.SearchTask(title, isCompleted, userId, isAdmin);

        return tasks.Select(t => MapTaskItemToResponseDto(t));
    }
    private ResponseTaskDto MapTaskItemToResponseDto(TaskItem taskItem)
    {
        return new ResponseTaskDto(Id: taskItem.Id,
        Title: taskItem.Title, Description: taskItem.Description, IsCompleted: taskItem.IsCompleted, CreatedAt: taskItem.CreatedAt);
    }

    private TaskItem MapCreateRequestDtoToTaskItem(CreateTaskDto createTaskDto, Guid userId)
    {
        return new TaskItem()
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            UserId = userId
        };
    }

    private TaskItem MapUpdateTaskDtoToTaskItem(UpdateTaskDto updateTaskDto, Guid taskId, Guid userId)
    {
        return new TaskItem()
        {
            Id = taskId,
            Title = updateTaskDto.Title,
            Description = updateTaskDto.Description,
            IsCompleted = updateTaskDto.IsCompleted,
            UserId = userId
        };
    }
}
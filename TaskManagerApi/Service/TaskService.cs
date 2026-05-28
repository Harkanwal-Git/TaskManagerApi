using System.IdentityModel.Tokens.Jwt;
using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TaskService> _logger;
    public TaskService(ITaskRepository taskRepository, ITagRepository tagRepository, ILogger<TaskService> logger)
    {
        this._taskRepository = taskRepository;
        this._tagRepository = tagRepository;
        this._logger = logger;
    }
    public async Task<ResponseTaskDto> AddTask(CreateTaskDto createTaskDto, Guid userId, bool isAdmin, CancellationToken ct)
    {
        if (!isAdmin && createTaskDto.AssignedUserId.HasValue)
        {
            _logger.LogWarning("User: {UserId} attempting to add task for other user: {TaskAssignee}", userId, createTaskDto.AssignedUserId);
            throw new UnauthorizedAccessException("Only admins can assign tasks to other users");
        }
        ResponseTaskDto responseTaskDto;
        var taskAssignee = isAdmin && createTaskDto.AssignedUserId.HasValue ? createTaskDto.AssignedUserId.Value : userId;
        TaskItem taskItem = await _taskRepository.AddTask(MapCreateRequestDtoToTaskItem(createTaskDto, taskAssignee), new OutboxMessage() { Payload = "" }, ct);
        _logger.LogInformation("User: {UserId} added task for user: {TaskAssignee}", userId, taskAssignee);
        responseTaskDto = MapTaskItemToResponseDto(taskItem);
        return responseTaskDto;
    }

    public async Task<IEnumerable<ResponseTaskDto>> GetAllTasks(Guid userId, bool isAdmin, CancellationToken ct)
    {
        _logger.LogInformation("Retreiving all tasks for user: {UserId}", userId);
        var result = await _taskRepository.GetAllTasks(userId, isAdmin, ct);

        return result.Select(t => MapTaskItemToResponseDto(t));
    }

    public async Task<ResponseTaskDto?> GetTaskById(Guid id, Guid userId, bool isAdmin, CancellationToken ct)
    {
        _logger.LogInformation("Retreiving task details for task: {TaskId}", id);
        TaskItem? taskItem = await _taskRepository.GetTaskById(id, userId, isAdmin, ct);

        return taskItem != null ? MapTaskItemToResponseDto(taskItem) : null;
    }

    public async Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin, CancellationToken ct)
    {
        _logger.LogInformation("Proceeding to delete task: {TaskId}, DeletedBy: {UserId}, Role: {Role}", taskId, userId, isAdmin ? "Admin" : "TaskOwner");
        return await _taskRepository.DeleteTask(taskId, userId, isAdmin, ct);
    }
    public async Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId, Guid userId, bool isAdmin, CancellationToken ct)
    {
        var taskItem = MapUpdateTaskDtoToTaskItem(updateTaskDto, taskId, userId);
        _logger.LogInformation("Task: {TaskId}, UpdatedBy: {UserId}, Role: {Role}", taskId, userId, isAdmin ? "Admin" : "TaskOwner");
        var updatedTaskItem = await _taskRepository.UpdateTask(taskItem, isAdmin, ct);

        return updatedTaskItem != null ? MapTaskItemToResponseDto(updatedTaskItem) : null;
    }

    public async Task<IEnumerable<ResponseTaskDto>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin, string? tagName, CancellationToken ct)
    {
        _logger.LogInformation("Quering tasks: #Title:{Title} #IsCompleted: {IsCompleted} #UserId: {UserId} #Role: {Role} #TagName: {TagName} ",
        title, isCompleted, userId, isAdmin ? "Admin" : "TaskOwner", tagName);
        IEnumerable<TaskItem> tasks = await _taskRepository.SearchTask(title, isCompleted, userId, isAdmin, tagName, ct);

        return tasks.Select(t => MapTaskItemToResponseDto(t));
    }
    private ResponseTaskDto MapTaskItemToResponseDto(TaskItem taskItem)
    {
        return new ResponseTaskDto(Id: taskItem.Id,
        Title: taskItem.Title, Description: taskItem.Description,
         IsCompleted: taskItem.IsCompleted, CreatedAt: taskItem.CreatedAt,
         UserId: taskItem.UserId, Tags: taskItem.TaskTags.Select(tt => MapTagToResponseTagDto(tt.Tag)).ToList());
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
    private ResponseTagDto MapTagToResponseTagDto(Tag tag)
    {
        return new ResponseTagDto(TagId: tag.Id, TagName: tag.TagName);
    }
    public async Task AddTaskTag(Guid taskId, Guid tagId, bool isAdmin, Guid userId, CancellationToken ct)
    {
        var task = await _taskRepository.GetTaskById(taskId, userId, isAdmin, ct);
        if (task == null)
        {
            _logger.LogWarning("No Task found for TaskId {TaskId} to add the tag", taskId);
            throw new KeyNotFoundException();
        }
        var tag = await _tagRepository.GetTagById(tagId, ct);

        if (tag == null)
        {
            _logger.LogWarning("No Tag found for  TagId {TagId} to add the task", tagId);
            throw new KeyNotFoundException();
        }

        await _taskRepository.AddTaskTag(taskId, tagId, ct);

    }

    public async Task<bool> RemoveTaskTag(Guid taskId, Guid tagId, bool isAdmin, Guid userId, CancellationToken ct)
    {
        _logger.LogInformation("Proceeding to remove Tag : {TagId} for Task: {TaskId}", tagId, taskId);
        return await _taskRepository.RemoveTaskTag(taskId, tagId, isAdmin, userId, ct);
    }


}
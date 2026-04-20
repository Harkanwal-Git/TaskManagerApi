using TaskManagerApi.DTO;


public interface ITaskService
{
    public Task<IEnumerable<ResponseTaskDto>> GetAllTasks(Guid userId, bool isAdmin, CancellationToken ct);
    public Task<ResponseTaskDto?> GetTaskById(Guid id, Guid userId, bool isAdmin, CancellationToken ct);
    public Task<ResponseTaskDto> AddTask(CreateTaskDto task, Guid userId, bool isAdmin, CancellationToken ct);

    public Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin, CancellationToken ct);
    public Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId, Guid userId, bool isAdmin, CancellationToken ct);

    public Task<IEnumerable<ResponseTaskDto>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin, string? tagName, CancellationToken ct);

    public Task AddTaskTag(Guid taskId, Guid TagId, bool isAdmin, Guid userId, CancellationToken ct);

    public Task<bool> RemoveTaskTag(Guid taskId, Guid tagId, bool isAdmin, Guid userId, CancellationToken ct);
}
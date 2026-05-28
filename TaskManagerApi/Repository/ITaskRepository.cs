using TaskManagerApi.DTO;
using TaskManagerApi.Model;
namespace TaskManagerApi.Repository;

public interface ITaskRepository
{
    public Task<IEnumerable<TaskItem>> GetAllTasks(Guid userId, bool isAdmin, CancellationToken ct);
    public Task<TaskItem?> GetTaskById(Guid Id, Guid userId, bool isAdmin, CancellationToken ct);
    public Task<TaskItem> AddTask(TaskItem task, OutboxMessage outboxMessage, CancellationToken ct);

    public Task<TaskItem?> UpdateTask(TaskItem task, bool isAdmin, CancellationToken ct);

    public Task<bool> DeleteTask(Guid guid, Guid userId, bool isAdmin, CancellationToken ct);

    public Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin, string? tagName, CancellationToken ct);

    public Task AddTaskTag(Guid taskId, Guid TagId, CancellationToken ct);

    public Task<bool> RemoveTaskTag(Guid taskId, Guid tagId, bool isAdmin, Guid userId, CancellationToken ct);

}
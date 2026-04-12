using TaskManagerApi.DTO;
using TaskManagerApi.Model;
namespace TaskManagerApi.Repository;

public interface ITaskRepository
{
    public Task<IEnumerable<TaskItem>> GetAllTasks(Guid userId, bool isAdmin);
    public Task<TaskItem?> GetTaskById(Guid Id, Guid userId, bool isAdmin);
    public Task<TaskItem> AddTask(TaskItem task);

    public Task<TaskItem?> UpdateTask(TaskItem task, bool isAdmin);

    public Task<bool> DeleteTask(Guid guid, Guid userId, bool isAdmin);

    public Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin);
}
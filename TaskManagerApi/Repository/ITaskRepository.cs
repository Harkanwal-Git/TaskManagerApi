using TaskManagerApi.DTO;
using TaskManagerApi.Model;
namespace TaskManagerApi.Repository;

public interface ITaskRepository
{
    public Task<IEnumerable<TaskItem>> GetAllTasks();
    public Task<TaskItem?> GetTaskById(Guid Id);
    public Task<TaskItem> AddTask(TaskItem task);

    public Task<TaskItem?> UpdateTask(TaskItem task);

    public Task<bool> DeleteTask(Guid guid);

    public Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted);
}
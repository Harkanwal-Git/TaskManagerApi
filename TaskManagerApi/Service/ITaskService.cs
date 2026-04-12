using TaskManagerApi.DTO;


public interface ITaskService
{
    public Task<IEnumerable<ResponseTaskDto>> GetAllTasks(Guid userId, bool isAdmin);
    public Task<ResponseTaskDto?> GetTaskById(Guid id, Guid userId, bool isAdmin);
    public Task<ResponseTaskDto> AddTask(CreateTaskDto task, Guid userId, bool isAdmin);

    public Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin);
    public Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId, Guid userId, bool isAdmin);

    public Task<IEnumerable<ResponseTaskDto>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin);
}
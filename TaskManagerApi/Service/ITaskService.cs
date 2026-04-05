using TaskManagerApi.DTO;
using TaskManagerApi.Model;

public interface ITaskService
{
    public Task<IEnumerable<ResponseTaskDto>> GetAllTasks();
    public Task<ResponseTaskDto?> GetTaskById(Guid id);
    public Task<ResponseTaskDto> AddTask(CreateTaskDto task);

    public Task<bool> DeleteTask(Guid taskId);
    public Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId);
}
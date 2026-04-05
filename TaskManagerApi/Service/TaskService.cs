using System.Threading.Tasks;
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
    public async Task<ResponseTaskDto> AddTask(CreateTaskDto createTaskDto)
    {
        ResponseTaskDto responseTaskDto;

        TaskItem taskItem = await _taskRepository.AddTask(MapCreateRequestDtoToTaskItem(createTaskDto));
        responseTaskDto = MapTaskItemToResponseDto(taskItem);
        return responseTaskDto;
    }

    public async Task<IEnumerable<ResponseTaskDto>> GetAllTasks()
    {
        var result = await _taskRepository.GetAllTasks();
        return result.Select(t => MapTaskItemToResponseDto(t));
    }

    public async Task<ResponseTaskDto?> GetTaskById(Guid id)
    {
        TaskItem? taskItem = await _taskRepository.GetTaskById(id);

        return taskItem != null ? MapTaskItemToResponseDto(taskItem) : null;
    }

    public async Task<bool> DeleteTask(Guid taskId)
    {
        return await _taskRepository.DeleteTask(taskId);
    }
    public async Task<ResponseTaskDto?> UpdateTask(UpdateTaskDto updateTaskDto, Guid taskId)
    {
        var taskItem = MapUpdateTaskDtoToTaskItem(updateTaskDto, taskId);
        var updatedTaskItem = await _taskRepository.UpdateTask(taskItem);

        return updatedTaskItem != null ? MapTaskItemToResponseDto(updatedTaskItem) : null;
    }
    private ResponseTaskDto MapTaskItemToResponseDto(TaskItem taskItem)
    {
        return new ResponseTaskDto(Id: taskItem.Id,
        Title: taskItem.Title, Description: taskItem.Description, IsCompleted: taskItem.IsCompleted, CreatedAt: taskItem.CreatedAt);
    }

    private TaskItem MapCreateRequestDtoToTaskItem(CreateTaskDto createTaskDto)
    {
        return new TaskItem()
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description
        };
    }

    private TaskItem MapUpdateTaskDtoToTaskItem(UpdateTaskDto updateTaskDto, Guid taskId)
    {
        return new TaskItem()
        {
            Id = taskId,
            Title = updateTaskDto.Title,
            Description = updateTaskDto.Description,
            IsCompleted = updateTaskDto.IsCompleted
        };
    }
}
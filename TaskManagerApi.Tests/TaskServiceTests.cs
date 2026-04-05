using TaskManagerApi.Model;
using TaskManagerApi.Repository;
using Moq;
using TaskManagerApi.Service;
using TaskManagerApi.DTO;

namespace TaskManagerApi.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly TaskService _taskService;
    public TaskServiceTests()
    {
        _mockRepository = new();
        _taskService = new TaskService(_mockRepository.Object);
    }
    [Fact]
    public async Task AddTask_WithValidInputs_ReturnsCreatedTask()
    {
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description");
        _mockRepository.Setup(r => r.AddTask(It.IsAny<TaskItem>()))
                        .ReturnsAsync((TaskItem task) => task
                    );

        var result = await _taskService.AddTask(createTaskDto);

        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);

        _mockRepository.Verify(s => s.AddTask(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsIEnumerableTasks()
    {
        _mockRepository.Setup(r => r.GetAllTasks())
        .ReturnsAsync(new List<TaskItem>()
        {
            new TaskItem(){Title="Test",Description="test description"},
            new TaskItem(){Title="Test2",Description="test description2"},
            new TaskItem(){Title="Test3",Description="test description3"},
        });

        var result = await _taskService.GetAllTasks();

        Assert.NotEmpty(result);
        _mockRepository.Verify(r => r.GetAllTasks(), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_WhenEmpty_ReturnsEmptyList()
    {
        // Given
        _mockRepository.Setup(r => r.GetAllTasks())
                        .ReturnsAsync(new List<TaskItem>());
        // When
        var result = await _taskService.GetAllTasks();
        // Then

        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetAllTasks(), Times.Once);
    }

    [Fact]
    public async Task GetTasksById_WithExistingIdReturnsTask()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid id) => new TaskItem
                        {
                            Id = id,
                            Title = "title",
                            Description = "Description",
                        });
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>()), Times.Once);
    }
    [Fact]
    public async Task GetTasksById_WithNonExistingIdReturnsNull()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>()))
                               .ReturnsAsync((Guid id) => null);
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId);

        Assert.Null(result);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithValidId_ReturnsUpdatedTask()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>()))
                               .ReturnsAsync((TaskItem taskItem) => taskItem);
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithInValidId_ReturnsNull()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>()))
                               .ReturnsAsync((TaskItem taskItem) => null);
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId);

        Assert.Null(result);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnFalse()
    {
        _mockRepository.Setup(r => r.DeleteTask(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => false);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId);

        Assert.False(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithvalidId_ReturnTrue()
    {
        _mockRepository.Setup(s => s.DeleteTask(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => true);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId);

        Assert.True(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>()), Times.Once);

    }


}
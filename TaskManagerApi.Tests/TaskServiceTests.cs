using TaskManagerApi.Model;
using TaskManagerApi.Repository;
using Moq;
using TaskManagerApi.Service;
using TaskManagerApi.DTO;
using System.Reflection.Metadata;

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

        var result = await _taskService.AddTask(createTaskDto, Guid.NewGuid(), false);

        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);

        _mockRepository.Verify(s => s.AddTask(It.IsAny<TaskItem>()), Times.Once);
    }
    [Fact]
    public async Task AddTask_AsAdmin_WithAssignedUserId_AssignsToSpecifiedUser()
    {
        Guid assignedUserId = Guid.NewGuid();
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description", AssignedUserId: assignedUserId);
        _mockRepository.Setup(r => r.AddTask(It.IsAny<TaskItem>()))
                        .ReturnsAsync((TaskItem task) => task);

        var result = await _taskService.AddTask(createTaskDto, Guid.NewGuid(), true);

        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);
        Assert.Equal(assignedUserId, result.UserId);
        _mockRepository.Verify(r => r.AddTask(It.IsAny<TaskItem>()), Times.Once);
    }
    [Fact]
    public async Task AddTask_AsRegularUser_WithAssignedUserId_ThrowsUnauthorized()
    {
        Guid assignedUserId = Guid.NewGuid();
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description", AssignedUserId: assignedUserId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _taskService.AddTask(createTaskDto, Guid.NewGuid(), false));
    }

    [Fact]
    public async Task GetAllTasks_ReturnsIEnumerableTasks()
    {
        _mockRepository.Setup(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>()))
        .ReturnsAsync(new List<TaskItem>()
        {
            new TaskItem(){Title="Test",Description="test description",UserId=Guid.NewGuid()},
            new TaskItem(){Title="Test2",Description="test description2",UserId=Guid.NewGuid()},
            new TaskItem(){Title="Test3",Description="test description3",UserId=Guid.NewGuid()},
        });

        var result = await _taskService.GetAllTasks(Guid.NewGuid(), true);

        Assert.NotEmpty(result);
        _mockRepository.Verify(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_WhenEmpty_ReturnsEmptyList()
    {
        // Given
        _mockRepository.Setup(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>()))
                        .ReturnsAsync(new List<TaskItem>());
        // When
        var result = await _taskService.GetAllTasks(Guid.NewGuid(), false);
        // Then

        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetTasksById_WithExistingIdReturnsTask()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                        .ReturnsAsync((Guid id, Guid userId, bool isAdmin) => new TaskItem
                        {
                            Id = id,
                            Title = "title",
                            Description = "Description",
                            UserId = userId
                        });
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId, Guid.NewGuid(), false);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
    }
    [Fact]
    public async Task GetTasksById_WithNonExistingIdReturnsNull()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                               .ReturnsAsync((Guid id, Guid userId, bool isAdmin) => null);
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId, Guid.NewGuid(), false);

        Assert.Null(result);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithValidId_ReturnsUpdatedTask()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>()))
                               .ReturnsAsync((TaskItem taskItem, bool isAdmin) => taskItem);
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId, Guid.NewGuid(), false);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithInValidId_ReturnsNull()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>()))
                               .ReturnsAsync((TaskItem taskItem, bool isAdmin) => null);
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId, Guid.NewGuid(), false);

        Assert.Null(result);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnFalse()
    {
        _mockRepository.Setup(r => r.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                        .ReturnsAsync((Guid guid, Guid userId, bool isAdmin) => false);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId, Guid.NewGuid(), false);

        Assert.False(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithvalidId_ReturnTrue()
    {
        _mockRepository.Setup(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                        .ReturnsAsync((Guid guid, Guid userId, bool isAdmin) => true);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId, Guid.NewGuid(), false);

        Assert.True(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);

    }


}
using TaskManagerApi.Model;
using TaskManagerApi.Repository;
using Moq;
using TaskManagerApi.Service;
using TaskManagerApi.DTO;
using Microsoft.Extensions.Logging;

namespace TaskManagerApi.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;

    private readonly Mock<ILogger<TaskService>> _mockTaskServiceLogger;
    private readonly TaskService _taskService;
    public TaskServiceTests()
    {
        _mockRepository = new();
        _mockTagRepository = new();
        _mockTaskServiceLogger = new();
        _taskService = new TaskService(_mockRepository.Object, _mockTagRepository.Object, _mockTaskServiceLogger.Object);
    }
    [Fact]
    public async Task AddTask_WithValidInputs_ReturnsCreatedTask()
    {
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description");
        _mockRepository.Setup(r => r.AddTask(It.IsAny<TaskItem>(), It.IsAny<OutboxMessage>(), CancellationToken.None))
                        .ReturnsAsync((TaskItem task, OutboxMessage outboxMessage, CancellationToken ct) => task
                    );

        var result = await _taskService.AddTask(createTaskDto, Guid.NewGuid(), false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);

        _mockRepository.Verify(s => s.AddTask(It.IsAny<TaskItem>(), It.IsAny<OutboxMessage>(), CancellationToken.None), Times.Once);
    }
    [Fact]
    public async Task AddTask_AsAdmin_WithAssignedUserId_AssignsToSpecifiedUser()
    {
        Guid assignedUserId = Guid.NewGuid();
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description", AssignedUserId: assignedUserId);
        _mockRepository.Setup(r => r.AddTask(It.IsAny<TaskItem>(), It.IsAny<OutboxMessage>(), CancellationToken.None))
                        .ReturnsAsync((TaskItem task, OutboxMessage outboxMessage, CancellationToken ct) => task);

        var result = await _taskService.AddTask(createTaskDto, Guid.NewGuid(), true, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);
        Assert.Equal(assignedUserId, result.UserId);
        _mockRepository.Verify(r => r.AddTask(It.IsAny<TaskItem>(), It.IsAny<OutboxMessage>(), CancellationToken.None), Times.Once);
    }
    [Fact]
    public async Task AddTask_AsRegularUser_WithAssignedUserId_ThrowsUnauthorized()
    {
        Guid assignedUserId = Guid.NewGuid();
        CreateTaskDto createTaskDto = new(Title: "Test Task", Description: "Test Task Description", AssignedUserId: assignedUserId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _taskService.AddTask(createTaskDto, Guid.NewGuid(), false, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllTasks_ReturnsIEnumerableTasks()
    {
        _mockRepository.Setup(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<TaskItem>()
        {
            new TaskItem(){Title="Test",Description="test description",UserId=Guid.NewGuid()},
            new TaskItem(){Title="Test2",Description="test description2",UserId=Guid.NewGuid()},
            new TaskItem(){Title="Test3",Description="test description3",UserId=Guid.NewGuid()},
        });

        var result = await _taskService.GetAllTasks(Guid.NewGuid(), true, CancellationToken.None);

        Assert.NotEmpty(result);
        _mockRepository.Verify(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_WhenEmpty_ReturnsEmptyList()
    {
        // Given
        _mockRepository.Setup(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<TaskItem>());
        // When
        var result = await _taskService.GetAllTasks(Guid.NewGuid(), false, CancellationToken.None);
        // Then

        Assert.NotNull(result);
        Assert.Empty(result);

        _mockRepository.Verify(r => r.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTasksById_WithExistingIdReturnsTask()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync((Guid id, Guid userId, bool isAdmin, CancellationToken ct) => new TaskItem
                        {
                            Id = id,
                            Title = "title",
                            Description = "Description",
                            UserId = userId
                        });
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }
    [Fact]
    public async Task GetTasksById_WithNonExistingIdReturnsNull()
    {
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                               .ReturnsAsync((TaskItem?)null);
        var inputId = Guid.NewGuid();
        var result = await _taskService.GetTaskById(inputId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.Null(result);

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithValidId_ReturnsUpdatedTask()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>(), CancellationToken.None))
                               .ReturnsAsync((TaskItem taskItem, bool isAdmin, CancellationToken ct) => taskItem);
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(inputId, result.Id);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithInValidId_ReturnsNull()
    {
        _mockRepository.Setup(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>(), CancellationToken.None))
                               .ReturnsAsync(default(TaskItem));
        var inputId = Guid.NewGuid();
        var result = await _taskService.UpdateTask(new UpdateTaskDto(Title: "Test", Description: "Testing", IsCompleted: true), inputId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.Null(result);

        _mockRepository.Verify(r => r.UpdateTask(It.IsAny<TaskItem>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnFalse()
    {
        _mockRepository.Setup(r => r.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync(false);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.False(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithvalidId_ReturnTrue()
    {
        _mockRepository.Setup(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync(true);

        var taskId = Guid.NewGuid();
        var result = await _taskService.DeleteTask(taskId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.True(result);

        _mockRepository.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }
    [Fact]
    public async Task AddTaskTag_WithValidTaskIdandTagId()
    {
        // Given
        Guid capturedTaskId = default;
        Guid capturedTagId = default;
        _mockRepository.Setup(t => t.AddTaskTag(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .Callback<Guid, Guid, CancellationToken>((taskId, tagId, ct) =>
                        {
                            capturedTagId = tagId;
                            capturedTaskId = taskId;
                        });
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Guid id, Guid userId, bool isAdmin, CancellationToken ct) => new TaskItem
                        {
                            Id = id,
                            Title = "title",
                            Description = "Description",
                            UserId = userId
                        });
        _mockTagRepository.Setup(tg => tg.GetTagById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Guid tagId, CancellationToken ct) => new Tag { Id = tagId, TagName = "test-tag" });
        // When
        Guid taskId = Guid.NewGuid();
        Guid tagId = Guid.NewGuid();
        await _taskService.AddTaskTag(taskId, tagId, false, Guid.NewGuid(), CancellationToken.None);

        // Then
        Assert.Equal(capturedTagId, tagId);
        Assert.Equal(capturedTaskId, taskId);
    }
    [Fact]
    public async Task AddTaskTag_WithValidInvalidTaskIdorTagId_ThrowsKeyNotFound()
    {
        // When
        Guid taskId = Guid.NewGuid();
        Guid tagId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((TaskItem?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _taskService.AddTaskTag(taskId, tagId, false, Guid.NewGuid(), CancellationToken.None));

        _mockRepository.Verify(r => r.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTaskTag_WithValidTaskAndTag_ReturnsTrue()
    {
        // Given
        _mockRepository.Setup(t => t.RemoveTaskTag(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);
        // When
        var result = await _taskService.RemoveTaskTag(Guid.NewGuid(), Guid.NewGuid(), true, Guid.NewGuid(), CancellationToken.None);
        // Then
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveTaskTag_WithInValidTaskAndTag_ReturnsFalse()
    {
        // Given
        _mockRepository.Setup(t => t.RemoveTaskTag(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);
        // When
        var result = await _taskService.RemoveTaskTag(Guid.NewGuid(), Guid.NewGuid(), true, Guid.NewGuid(), CancellationToken.None);
        // Then
        Assert.False(result);
    }


}
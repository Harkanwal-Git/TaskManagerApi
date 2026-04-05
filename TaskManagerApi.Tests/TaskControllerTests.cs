using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagerApi.Controllers;
using TaskManagerApi.DTO;

namespace TaskManagerApi.Tests;

public class TaskControllerTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly TasksController _taskController;
    public TaskControllerTests()
    {
        _mockTaskService = new();
        _taskController = new TasksController(_mockTaskService.Object);

    }
    [Fact]
    public async Task GetAllTasks_WithExistingTasks_Returns200WithTasks()
    {
        _mockTaskService.Setup(s => s.GetAllTasks())
                        .ReturnsAsync(new List<ResponseTaskDto>()
                        {
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title",Description:"Test Description",CreatedAt:DateTime.UtcNow),
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title2",Description:"Test Description2",CreatedAt:DateTime.UtcNow),
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title3",Description:"Test Description3",CreatedAt:DateTime.UtcNow)

                        });
        var result = await _taskController.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var tasks = Assert.IsAssignableFrom<IEnumerable<ResponseTaskDto>>(okResult.Value);


        Assert.NotEmpty(tasks);

        _mockTaskService.Verify(s => s.GetAllTasks(), Times.Once);
    }

    [Fact]
    public async Task AddTask_WithValidInput_Returns201WithCreatedTask()
    {
        _mockTaskService.Setup(s => s.AddTask(It.IsAny<CreateTaskDto>()))
                        .ReturnsAsync((CreateTaskDto createTask) => new ResponseTaskDto(
                            Id: Guid.NewGuid(), Title: createTask.Title, Description: createTask.Description, CreatedAt: DateTime.UtcNow)
                            );
        CreateTaskDto createTaskDto = new(Title: "Test", Description: "Test Description");
        var result = await _taskController.AddTask(createTaskDto);

        var CreatedAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(CreatedAtActionResult.Value);

        _mockTaskService.Verify(s => s.AddTask(It.IsAny<CreateTaskDto>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_WhenEmpty_Returns200WithEmptyList()
    {
        _mockTaskService.Setup(s => s.GetAllTasks())
                        .ReturnsAsync(new List<ResponseTaskDto>());

        var result = await _taskController.GetAll();

        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);

        var tasks = Assert.IsAssignableFrom<IEnumerable<ResponseTaskDto>>(okObjectResult.Value);

        Assert.Empty(tasks);

        _mockTaskService.Verify(s => s.GetAllTasks(), Times.Once);
    }

    [Fact]
    public async Task GetTaskById_WithNonExistingId_Returns404()
    {
        _mockTaskService.Setup(s => s.GetTaskById(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => null);

        var result = await _taskController.GetById(Guid.NewGuid());
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

        _mockTaskService.Verify(s => s.GetTaskById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskById_WithExistingId_Returns200WithTask()
    {
        _mockTaskService.Setup(s => s.GetTaskById(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => new ResponseTaskDto(Id: guid, Title: "title", Description: "Description", CreatedAt: DateTime.UtcNow));

        Guid guid = Guid.NewGuid();
        var result = await _taskController.GetById(guid);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(okResult.Value);

        Assert.Equal(guid, task.Id);

        _mockTaskService.Verify(s => s.GetTaskById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithValidId_Return200WithUpdatedTask()
    {
        _mockTaskService.Setup(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>()))
                        .ReturnsAsync((UpdateTaskDto updateTaskDto, Guid guid) => new ResponseTaskDto(Id: guid, Title: updateTaskDto.Title, Description: updateTaskDto.Description, CreatedAt: DateTime.UtcNow, IsCompleted: updateTaskDto.IsCompleted));

        var taskId = Guid.NewGuid();
        var result = await _taskController.UpdateTask(new UpdateTaskDto("Test", "Testing", true), taskId);

        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(okObjectResult.Value);

        Assert.Equal(taskId, task.Id);
        _mockTaskService.Verify(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_Return404NotFound()
    {
        _mockTaskService.Setup(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>()))
                        .ReturnsAsync((UpdateTaskDto updateTaskDto, Guid guid) => null);

        var taskId = Guid.NewGuid();
        var result = await _taskController.UpdateTask(new UpdateTaskDto("Test", "Testing", true), taskId);

        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);

        _mockTaskService.Verify(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_Return404NotFound()
    {
        _mockTaskService.Setup(s => s.DeleteTask(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => false);

        var taskId = Guid.NewGuid();
        var result = await _taskController.DeleteTask(taskId);

        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);

        _mockTaskService.Verify(s => s.DeleteTask(It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithvalidId_Return204NoContent()
    {
        _mockTaskService.Setup(s => s.DeleteTask(It.IsAny<Guid>()))
                        .ReturnsAsync((Guid guid) => true);

        var taskId = Guid.NewGuid();
        var result = await _taskController.DeleteTask(taskId);

        var noContentResult = Assert.IsType<NoContentResult>(result);

        _mockTaskService.Verify(s => s.DeleteTask(It.IsAny<Guid>()), Times.Once);

    }


}
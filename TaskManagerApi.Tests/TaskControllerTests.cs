using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagerApi.Controllers;
using TaskManagerApi.CQRS.Commands;
using TaskManagerApi.CQRS.Queries;
using TaskManagerApi.DTO;
using TaskManagerApi.Model;

namespace TaskManagerApi.Tests;

public class TaskControllerTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<IMediator> _mockMediator;

    private readonly TasksController _taskController;
    private readonly Guid _expectedUserId = default;
    public TaskControllerTests()
    {
        _mockTaskService = new();
        _mockMediator = new();
        _taskController = new TasksController(_mockTaskService.Object, _mockMediator.Object);

        _expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,_expectedUserId.ToString()),
            new Claim(ClaimTypes.Role,"Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        _taskController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

    }
    [Fact]
    public async Task GetAllTasks_WithExistingTasks_Returns200WithTasks()
    {
        _mockTaskService.Setup(s => s.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync((Guid userId, bool isAdmin, CancellationToken ct) => new List<ResponseTaskDto>()
                        {
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title",Description:"Test Description",CreatedAt:DateTime.UtcNow,UserId:userId,IsCompleted:false,Tags:new List<ResponseTagDto>(){new ResponseTagDto(TagId:Guid.NewGuid(),TagName:"Test Tag")}),
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title2",Description:"Test Description2",CreatedAt:DateTime.UtcNow,UserId:userId,IsCompleted:false,Tags:null),
                            new ResponseTaskDto(Id:Guid.NewGuid(),Title:"Test Title3",Description:"Test Description3",CreatedAt:DateTime.UtcNow,UserId:userId,IsCompleted:false,Tags:new List<ResponseTagDto>())

                        });
        var result = await _taskController.GetAll(CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var tasks = Assert.IsAssignableFrom<IEnumerable<ResponseTaskDto>>(okResult.Value);


        Assert.NotEmpty(tasks);

        _mockTaskService.Verify(s => s.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task AddTask_WithValidInput_Returns201WithCreatedTask()
    {
        // _mockTaskService.Setup(s => s.AddTask(It.IsAny<CreateTaskDto>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
        //                 .ReturnsAsync((CreateTaskDto createTask, Guid userId, bool isAdmin, CancellationToken ct) => new ResponseTaskDto(
        //                     Id: Guid.NewGuid(), Title: createTask.Title, Description: createTask.Description, CreatedAt: DateTime.UtcNow, userId, false, Tags: null)
        //                     );

        _mockMediator.Setup(s => s.Send(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ResponseTaskDto(Id: Guid.NewGuid(), Title: "Test", Description: "Test Description", CreatedAt: DateTime.UtcNow, Guid.NewGuid(), false, Tags: null));
        CreateTaskDto createTaskDto = new(Title: "Test", Description: "Test Description");
        var result = await _taskController.AddTask(createTaskDto, CancellationToken.None);

        var CreatedAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(CreatedAtActionResult.Value);

        // _mockTaskService.Verify(s => s.AddTask(It.IsAny<CreateTaskDto>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
        _mockMediator.Verify(s => s.Send(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasks_WhenEmpty_Returns200WithEmptyList()
    {
        Guid capturedUserId = default;
        bool capturedIsAdmin = false;
        _mockTaskService.Setup(s => s.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
        .Callback<Guid, bool, CancellationToken>((userId, isAdmin, ct) =>
        {
            capturedUserId = userId;
            capturedIsAdmin = isAdmin;
        })
                        .ReturnsAsync(new List<ResponseTaskDto>());

        var result = await _taskController.GetAll(CancellationToken.None);

        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);

        var tasks = Assert.IsAssignableFrom<IEnumerable<ResponseTaskDto>>(okObjectResult.Value);

        Assert.Equal(_expectedUserId, capturedUserId);
        Assert.True(capturedIsAdmin);
        Assert.Empty(tasks);

        _mockTaskService.Verify(s => s.GetAllTasks(It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetTaskById_WithNonExistingId_Returns404()
    {
        // _mockTaskService.Setup(s => s.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
        //                 .ReturnsAsync((ResponseTaskDto?)null);

        _mockMediator.Setup(s => s.Send(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ResponseTaskDto?)null);

        var result = await _taskController.GetById(Guid.NewGuid(), CancellationToken.None);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        _mockMediator.Verify(s => s.Send(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        // _mockTaskService.Verify(s => s.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetTaskById_WithExistingId_Returns200WithTask()
    {
        // Guid capturedTaskId = default;
        // Guid capturedUserId = default;
        // bool capturedIsAdmin = false;
        // _mockTaskService.Setup(s => s.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
        // .Callback<Guid, Guid, bool, CancellationToken>((taskId, userId, isAdmin, ct) =>
        // {
        //     capturedTaskId = taskId;
        //     capturedUserId = userId;
        //     capturedIsAdmin = isAdmin;

        // })
        //                 .ReturnsAsync((Guid guid, Guid userId, bool isAdmin, CancellationToken ct) => new ResponseTaskDto(Id: guid, Title: "title", Description: "Description", CreatedAt: DateTime.UtcNow, userId, false, Tags: null));

        // Guid guid = Guid.NewGuid();
        // var result = await _taskController.GetById(guid, CancellationToken.None);

        // var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // var task = Assert.IsAssignableFrom<ResponseTaskDto>(okResult.Value);

        // Assert.Equal(guid, task.Id);
        // Assert.Equal(guid, capturedTaskId);
        // Assert.Equal(_expectedUserId, capturedUserId);
        // Assert.True(capturedIsAdmin);

        // _mockTaskService.Verify(s => s.GetTaskById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

        Guid taskId = Guid.NewGuid();
        var expectedTask = new ResponseTaskDto(Id: taskId, Title: "title", Description: "Description",
            CreatedAt: DateTime.UtcNow, UserId: _expectedUserId, IsCompleted: false, Tags: null);

        _mockMediator.Setup(s => s.Send(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedTask);

        var result = await _taskController.GetById(taskId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(okResult.Value);

        Assert.Equal(taskId, task.Id);
        _mockMediator.Verify(s => s.Send(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithValidId_Return200WithUpdatedTask()
    {
        _mockTaskService.Setup(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync((UpdateTaskDto updateTaskDto, Guid guid, Guid userId, bool isAdmin, CancellationToken ct) => new ResponseTaskDto(Id: guid, Title: updateTaskDto.Title, Description: updateTaskDto.Description, CreatedAt: DateTime.UtcNow, IsCompleted: updateTaskDto.IsCompleted, UserId: Guid.NewGuid(), Tags: null));

        var taskId = Guid.NewGuid();
        var result = await _taskController.UpdateTask(new UpdateTaskDto("Test", "Testing", true), taskId, CancellationToken.None);

        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsAssignableFrom<ResponseTaskDto>(okObjectResult.Value);

        Assert.Equal(taskId, task.Id);
        _mockTaskService.Verify(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_Return404NotFound()
    {
        _mockTaskService.Setup(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync((ResponseTaskDto?)null);

        var taskId = Guid.NewGuid();
        var result = await _taskController.UpdateTask(new UpdateTaskDto("Test", "Testing", true), taskId, CancellationToken.None);

        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);

        _mockTaskService.Verify(s => s.UpdateTask(It.IsAny<UpdateTaskDto>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_Return404NotFound()
    {
        _mockTaskService.Setup(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync(false);

        var taskId = Guid.NewGuid();
        var result = await _taskController.DeleteTask(taskId, CancellationToken.None);

        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);

        _mockTaskService.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }

    [Fact]
    public async Task DeleteTask_WithvalidId_Return204NoContent()
    {
        _mockTaskService.Setup(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None))
                        .ReturnsAsync(true);

        var taskId = Guid.NewGuid();
        var result = await _taskController.DeleteTask(taskId, CancellationToken.None);

        var noContentResult = Assert.IsType<NoContentResult>(result);

        _mockTaskService.Verify(s => s.DeleteTask(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);

    }


}
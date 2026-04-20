using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.DTO;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService service)
    {
        _taskService = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResponseTaskDto>>> GetAll(CancellationToken ct)
    {
        var result = await _taskService.GetAllTasks(GetLoggedInUser(), User.IsInRole("Admin"), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseTaskDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _taskService.GetTaskById(id, GetLoggedInUser(), User.IsInRole("Admin"), ct);

        if (result == null)
        {
            return NotFound($"Task with id: {id} not found");
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ResponseTaskDto>> AddTask([FromBody] CreateTaskDto taskRequest, CancellationToken ct)
    {
        var responseTaskDto = await _taskService.AddTask(taskRequest, GetLoggedInUser(), User.IsInRole("Admin"), ct);
        return CreatedAtAction(nameof(GetById), new { id = responseTaskDto?.Id }, responseTaskDto);


    }

    [HttpPut("{Id:guid}")]
    public async Task<ActionResult<ResponseTaskDto>> UpdateTask([FromBody] UpdateTaskDto updateTaskDto, [FromRoute] Guid Id, CancellationToken ct)
    {
        var updatedTaskDto = await _taskService.UpdateTask(updateTaskDto, Id, GetLoggedInUser(), User.IsInRole("Admin"), ct);

        if (updatedTaskDto == null) return NotFound($"No task with Id: {Id} found to update");

        return Ok(updatedTaskDto);
    }

    [HttpDelete("{Id:guid}")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid Id, CancellationToken ct)
    {// Get current user's id from JWT claims
     // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     // var email = User.FindFirst(ClaimTypes.Email)?.Value;
     // var role = User.FindFirst(ClaimTypes.Role)?.Value;
     // System.Console.WriteLine($"{userId} {email} {role}");

        var deleted = await _taskService.DeleteTask(Id, GetLoggedInUser(), User.IsInRole("Admin"), ct);

        if (!deleted) return NotFound($"No tasks with Id: {Id} exists");

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ResponseTaskDto>>> SearchTask([FromQuery] string? title, [FromQuery] bool? isCompleted, [FromQuery] string? tagName, CancellationToken ct)
    {
        var tasks = await _taskService.SearchTask(title, isCompleted, GetLoggedInUser(), User.IsInRole("Admin"), tagName, ct);

        return Ok(tasks);
    }

    [HttpPost("{taskId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToTask([FromRoute] Guid taskId, [FromRoute] Guid tagId, CancellationToken ct)
    {
        await _taskService.AddTaskTag(taskId, tagId, User.IsInRole("Admin"), GetLoggedInUser(), ct);
        return Created();
    }

    [HttpDelete("{taskId}/tags/{tagId}")]
    public async Task<IActionResult> DeleteTagFromTask([FromRoute] Guid taskId, [FromRoute] Guid tagId, CancellationToken ct)
    {
        var deleted = await _taskService.RemoveTaskTag(taskId, tagId, User.IsInRole("Admin"), GetLoggedInUser(), ct);

        if (!deleted) return NotFound(new ProblemDetails { Status = 404, Title = "No Action happened", Detail = "Task or Tag Id doesnot exist", Instance = Request.Path });
        return NoContent();
    }



    private Guid GetLoggedInUser()
    {

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User Id claim not found");
        return Guid.TryParse(userIdString, out var userIdGuid) ? userIdGuid : throw new UnauthorizedAccessException("Invalid User Id Claim value");

    }
}
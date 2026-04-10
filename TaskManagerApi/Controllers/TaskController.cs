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
    public async Task<ActionResult<IEnumerable<ResponseTaskDto>>> GetAll()
    {
        var result = await _taskService.GetAllTasks();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseTaskDto>> GetById(Guid id)
    {
        var result = await _taskService.GetTaskById(id);

        if (result == null)
        {
            return NotFound($"Task with id: {id} not found");
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ResponseTaskDto>> AddTask([FromBody] CreateTaskDto taskRequest)
    {
        // if (string.IsNullOrWhiteSpace(taskRequest.Title))
        //     return BadRequest($"{nameof(taskRequest)} Title is required");

        try
        {
            var responseTaskDto = await _taskService.AddTask(taskRequest);
            return CreatedAtAction(nameof(GetById), new { id = responseTaskDto?.Id }, responseTaskDto);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 500);
        }
    }

    [HttpPut("{Id:guid}")]
    public async Task<ActionResult<ResponseTaskDto>> UpdateTask([FromBody] UpdateTaskDto updateTaskDto, [FromRoute] Guid Id)
    {
        var updatedTaskDto = await _taskService.UpdateTask(updateTaskDto, Id);

        if (updatedTaskDto == null) return NotFound($"No task with Id: {Id} found to update");

        return Ok(updatedTaskDto);
    }

    [HttpDelete("{Id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid Id)
    {// Get current user's id from JWT claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        System.Console.WriteLine($"{userId} {email} {role}");
        var deleted = await _taskService.DeleteTask(Id);

        if (!deleted) return NotFound($"No tasks with Id: {Id} exists");

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ResponseTaskDto>>> SearchTask([FromQuery] string? title, [FromQuery] bool? isCompleted)
    {
        var tasks = await _taskService.SearchTask(title, isCompleted);

        return Ok(tasks);
    }
}
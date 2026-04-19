using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.DTO;
using TaskManagerApi.Service;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResponseTagDto>>> GetTags()
    {
        var tags = await _tagService.GetTags();

        return Ok(tags);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseTagDto>> AddTag([FromBody] string tagName)
    {
        var tag = await _tagService.AddTag(tagName);

        return Created("", tag);
    }

    [HttpDelete("{tagId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid tagId)
    {
        var deleted = await _tagService.DeleteTag(tagId);

        if (!deleted) return NotFound(new ProblemDetails() { Title = "No Tag found for tag ID", Detail = $"No tag with tag Id : {tagId} exists", Instance = Request.Path, Status = 404 });

        return NoContent();
    }
}
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
    public async Task<ActionResult<IEnumerable<ResponseTagDto>>> GetTags(CancellationToken ct)
    {
        var tags = await _tagService.GetTags(ct);

        return Ok(tags);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseTagDto>> AddTag([FromBody] string tagName, CancellationToken ct)
    {  
        var tag = await _tagService.AddTag(tagName, ct);

        return Created("", tag);
    }

    [HttpDelete("{tagId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid tagId, CancellationToken ct)
    {
        var deleted = await _tagService.DeleteTag(tagId, ct);

        if (!deleted) return NotFound(new ProblemDetails() { Title = "No Tag found for tag ID", Detail = $"No tag with tag Id : {tagId} exists", Instance = Request.Path, Status = 404 });

        return NoContent();
    }
}
using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTO
{
    public record CreateTaskDto(
        [Required(ErrorMessage = "Title can't be empty/null")][MinLength(3, ErrorMessage = "Title should be minimum 3 characters long")] string Title, string? Description, Guid? AssignedUserId = null);

}
using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTO;

public record UpdateTaskDto([Required(ErrorMessage = "Title is a required field")][MinLength(3, ErrorMessage = "Title should be atleast three characters")] string Title, string? Description, bool IsCompleted);
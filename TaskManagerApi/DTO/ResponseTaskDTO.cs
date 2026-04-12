

namespace TaskManagerApi.DTO
{
    public record ResponseTaskDto(Guid Id, string Title, string? Description, DateTime CreatedAt, bool IsCompleted = false);

}
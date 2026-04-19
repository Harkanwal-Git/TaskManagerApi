

namespace TaskManagerApi.DTO
{
    public record ResponseTaskDto(Guid Id, string Title, string? Description, DateTime CreatedAt, Guid UserId, bool IsCompleted, List<ResponseTagDto>? Tags);

}
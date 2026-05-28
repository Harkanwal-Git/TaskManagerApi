namespace TaskManagerApi.Producers.Events;

public class TaskCreatedEvent
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime OccurredAt { get; set; }
}
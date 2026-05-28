using TaskManagerApi.Events;

namespace TaskManagerApi.Model;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public EventType EventType { get; set; }

    public required string Payload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? ProcessedAt { get; set; }
    public bool IsStalled { get; set; } = false;
}

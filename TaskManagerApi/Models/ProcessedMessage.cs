namespace TaskManagerApi.Model;

public class ProcessedMessage
{
    public Guid TaskId { get; set; }
    public DateTime ProcessedAt { get; set; }

    public required string ConsumerGroup { get; set; }
}
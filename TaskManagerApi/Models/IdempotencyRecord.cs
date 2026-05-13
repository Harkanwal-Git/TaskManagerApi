using System.Net;

namespace TaskManagerApi.Model;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public required Guid IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string RequestPayload { get; set; }
    public required string ResponsePayload { get; set; }
    public int StatusCode { get; set; }
    public DateTime ExpiresAt { get; set; }

}


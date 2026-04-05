namespace TaskManagerApi.Model
{
    public class TaskItem
    {

        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
namespace TaskManagerApi.Model
{
    public class TaskItem
    {

        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
namespace TaskManagerApi.Model;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}


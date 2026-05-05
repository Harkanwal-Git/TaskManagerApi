namespace TaskManagerApi.Model;

public class User : BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    // public Role Role { get; set; } = Role.User;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}


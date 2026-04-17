namespace TaskManagerApi.Model;

public class UserRole
{
    public Guid UserId { get; set; }

    public Role Role { get; set; }

    public User User { get; set; } = null!;     //navigation property
}
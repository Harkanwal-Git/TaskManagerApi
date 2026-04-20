using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface IUserRepository
{
    public Task<User> AddUser(User user, CancellationToken ct);
    public Task<User?> GetUserByEmail(string Email, CancellationToken ct);

    public Task<bool> UserExists(string Email, CancellationToken ct);

}
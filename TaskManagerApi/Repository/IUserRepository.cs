using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface IUserRepository
{
    public Task<User> AddUser(User user);
    public Task<User?> GetUserByEmail(string Email);

    public Task<bool> UserExists(string Email);

}
using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Exceptions;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IDataConnectionFactory _dataConnectionFactory;
    public UserRepository(AppDbContext dbContext, IDataConnectionFactory dataConnectionFactory)
    {
        this._dbContext = dbContext;
        this._dataConnectionFactory = dataConnectionFactory;
    }
    public async Task<User> AddUser(User user, CancellationToken ct)
    {
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(ct);
        return user;

    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken ct)
    {
        return await _dbContext.Users.Include(ur => ur.Roles).AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct)
    {


        using var dbconnection = _dataConnectionFactory.CreateConnection();
        var sqlParams = new DynamicParameters();
        sqlParams.Add("Email", email);
        var result = await dbconnection.ExecuteScalarAsync<int?>(new CommandDefinition(@"Select top 1 1 from Users where Email=@Email", sqlParams, cancellationToken: ct));

        return result.HasValue && result.Value == 1;

    }
}
using System.Data;
using Microsoft.Data.SqlClient;


namespace TaskManagerApi.Data;

public class SQLConnectionFactory : IDataConnectionFactory
{
    private readonly string _connectionString;
    public SQLConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }


}
using System.Data;
using System.Data.Common;

namespace TaskManagerApi.Data;

public interface IDataConnectionFactory
{
    IDbConnection CreateConnection();
}
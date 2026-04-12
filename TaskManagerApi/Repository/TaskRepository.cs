using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    private readonly IDataConnectionFactory _connectionFactory;
    public TaskRepository(AppDbContext dbContext, IDataConnectionFactory dataConnectionFactory)
    {
        this._dbContext = dbContext;
        this._connectionFactory = dataConnectionFactory;
    }

    public async Task<TaskItem> AddTask(TaskItem task)
    {

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin)
    {

        // var query = _dbContext.Tasks.Where(t => t.Id == taskId);
        // if (!isAdmin)
        //     query = query.Where(t => t.UserId == userId);

        // var rowsDeleted = await query.ExecuteDeleteAsync();

        var rowsDeleted = await _dbContext.Tasks.Where(t => t.Id == taskId && (isAdmin || t.UserId == userId)).ExecuteDeleteAsync();

        return rowsDeleted > 0;

    }

    public async Task<IEnumerable<TaskItem>> GetAllTasks(Guid userId, bool isAdmin)
    {
        return await _dbContext.Tasks.Where(t => isAdmin || t.UserId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<TaskItem?> GetTaskById(Guid Id, Guid userId, bool isAdmin)
    {
        return await _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == Id && (isAdmin || t.UserId == userId));
    }

    public async Task<TaskItem?> UpdateTask(TaskItem task, bool isAdmin)
    {
        var rowsAffected = await _dbContext.Tasks.Where(t => t.Id == task.Id && (isAdmin || t.UserId == task.UserId))
        .ExecuteUpdateAsync(
            s => s.SetProperty(t => t.Title, task.Title)
        .SetProperty(t => t.Description, task.Description)
        .SetProperty(t => t.IsCompleted, task.IsCompleted)
        );

        if (rowsAffected == 0) return null;
        return await _dbContext.Tasks.FindAsync(task.Id);

    }

    public async Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin)
    {
        var sql = "Select * from Tasks WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!isAdmin)
        {
            sql += " AND UserId=@UserId";
            parameters.Add("UserId", userId);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            sql += " AND Title LIKE @Title";
            parameters.Add("Title", $"%{title}%");
        }

        if (isCompleted.HasValue)
        {
            sql += " AND IsCompleted=@IsCompleted";
            parameters.Add("IsCompleted", isCompleted.Value);
        }


        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TaskItem>(sql, parameters);
    }
}
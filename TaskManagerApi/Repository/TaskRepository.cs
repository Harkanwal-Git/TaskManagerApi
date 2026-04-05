using System.Data;
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
    // private static List<TaskItem> _tasks = new List<TaskItem>();
    public async Task<TaskItem> AddTask(TaskItem task)
    {

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<bool> DeleteTask(Guid taskId)
    {

        // var task = await _dbContext.Tasks.FindAsync(taskId);

        // if (task == null) return false;

        // _dbContext.Tasks.Remove(task);

        // await _dbContext.SaveChangesAsync();
        // return true;

        var rowsDeleted = await _dbContext.Tasks.Where(t => t.Id == taskId).ExecuteDeleteAsync();

        return rowsDeleted > 0;

    }

    public async Task<IEnumerable<TaskItem>> GetAllTasks()
    {
        return await _dbContext.Tasks.AsNoTracking().ToListAsync();
    }

    public async Task<TaskItem?> GetTaskById(Guid Id)
    {
        return await _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == Id);
    }

    public async Task<TaskItem?> UpdateTask(TaskItem task)
    {
        #region otherapproach
        // var ogTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);
        // if (ogTask == null) return ogTask;

        // ogTask.Title = task.Title;
        // ogTask.Description = task.Description;
        // ogTask.IsCompleted = task.IsCompleted;

        // await _dbContext.SaveChangesAsync();
        // return ogTask;
        #endregion
        var rowsAffected = await _dbContext.Tasks.Where(t => t.Id == task.Id)
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.Title, task.Title)
        .SetProperty(t => t.Description, task.Description)
        .SetProperty(t => t.IsCompleted, task.IsCompleted));

        if (rowsAffected == 0) return null;
        return await _dbContext.Tasks.FindAsync(task.Id);

    }

    public async Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted)
    {
        var sql = "Select * from Tasks WHERE 1=1";
        var parameters = new DynamicParameters();

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
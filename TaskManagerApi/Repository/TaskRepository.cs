using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
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

    public async Task<TaskItem> AddTask(TaskItem task, CancellationToken ct)
    {

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(ct);

        return task;
    }

    public async Task<bool> DeleteTask(Guid taskId, Guid userId, bool isAdmin, CancellationToken ct)
    {

        // var query = _dbContext.Tasks.Where(t => t.Id == taskId);
        // if (!isAdmin)
        //     query = query.Where(t => t.UserId == userId);

        // var rowsDeleted = await query.ExecuteDeleteAsync();

        // var rowsDeleted = await _dbContext.Tasks.Where(t => t.Id == taskId && (isAdmin || t.UserId == userId)).ExecuteDeleteAsync(ct);

        var rowsSoftDeleted = await _dbContext.Tasks.Where(t => t.Id == taskId && (isAdmin || t.UserId == userId)).ExecuteUpdateAsync(t => t.SetProperty(t => t.IsDeleted, true));

        return rowsSoftDeleted > 0;

    }

    public async Task<IEnumerable<TaskItem>> GetAllTasks(Guid userId, bool isAdmin, CancellationToken ct)
    {
        return await _dbContext.Tasks.AsNoTracking().Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).Where(t => isAdmin || t.UserId == userId).ToListAsync(ct);
    }

    public async Task<TaskItem?> GetTaskById(Guid Id, Guid userId, bool isAdmin, CancellationToken ct)
    {
        return await _dbContext.Tasks.AsNoTracking().Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).FirstOrDefaultAsync(t => t.Id == Id && (isAdmin || t.UserId == userId), ct);
    }

    public async Task<TaskItem?> UpdateTask(TaskItem task, bool isAdmin, CancellationToken ct)
    {
        var rowsAffected = await _dbContext.Tasks.Where(t => t.Id == task.Id && (isAdmin || t.UserId == task.UserId))
        .ExecuteUpdateAsync(
            s => s.SetProperty(t => t.Title, task.Title)
        .SetProperty(t => t.Description, task.Description)
        .SetProperty(t => t.IsCompleted, task.IsCompleted)
       , ct);

        if (rowsAffected == 0) return null;
        // return await _dbContext.Tasks.FindAsync(task.Id);

        return await _dbContext.Tasks.AsNoTracking().Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).FirstOrDefaultAsync(t => t.Id == task.Id, ct);
    }

    public async Task<IEnumerable<TaskItem>> SearchTask(string? title, bool? isCompleted, Guid userId, bool isAdmin, string? tagName, CancellationToken ct)
    {
        var sql = @"Select * from Tasks WHERE 1=1";
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
        if (!string.IsNullOrWhiteSpace(tagName))
        {
            sql += @" AND EXISTS (
                    SELECT 1
                    FROM TaskTags tt
                    INNER JOIN Tags ta ON tt.TagId = ta.Id
                    WHERE tt.TaskId = Tasks.Id
                    AND ta.TagName LIKE @TagName
                 )";

            parameters.Add("TagName", $"%{tagName}%");
        }

        using var connection = _connectionFactory.CreateConnection();
        // Step 1: Get tasks
        var tasks = (await connection.QueryAsync<TaskItem>(new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
        if (!tasks.Any())
        {
            return tasks;
        }

        var taskIds = tasks.Select(t => t.Id).ToList();
        var multiSql = @"Select * from TaskTags Where TaskId IN @TaskIds;
                        Select * from Tags where Id IN (Select TagId from TaskTags Where TaskId IN @TaskIds)";


        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(multiSql, new { TaskIds = taskIds }, cancellationToken: ct));



        var taskTags = (await multi.ReadAsync<TaskTag>()).ToList();
        var tags = (await multi.ReadAsync<Tag>()).ToList();

        var tagLookUp = tags.ToDictionary(t => t.Id);

        var taskTagLookUp = taskTags
                        .GroupBy(tt => tt.TaskId)
                        .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var task in tasks)
        {
            if (taskTagLookUp.TryGetValue(task.Id, out var tts))
            {

                task.TaskTags = tts.Select(tt =>
                {
                    tt.Tag = tagLookUp[tt.TagId];
                    return tt;
                }).ToList();
            }
        }
        return tasks;
    }

    public async Task AddTaskTag(Guid taskId, Guid tagId, CancellationToken ct)
    {
        _dbContext.TaskTags.Add(new TaskTag() { TaskId = taskId, TagId = tagId });

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveTaskTag(Guid taskId, Guid tagId, bool isAdmin, Guid userId, CancellationToken ct)
    {
        var rowsDeleted = await _dbContext.TaskTags.Where(tt => tt.TaskId == taskId && tt.TagId == tagId && (isAdmin || tt.TaskItem.UserId == userId)).ExecuteDeleteAsync(cancellationToken: ct);

        return rowsDeleted > 0;
    }

    //For learning db transactions
    public async Task<TaskItem> CreateTaskWithTag(TaskItem task, string tagName, CancellationToken ct)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var tag = new Tag { TagName = tagName };
            _dbContext.Tags.Add(tag);
            await _dbContext.SaveChangesAsync(ct);

            task.TaskTags.Add(new TaskTag { TagId = tag.Id });
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return task;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<TaskItem> CreateTaskWithTag_Dapper(TaskItem task, string tagName, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var tag = new Tag { TagName = tagName };
            var insertTagSql = @"Insert into Tags (Id,TagName) Values (@Id, @TagName)";

            var insertedRows = await connection.ExecuteAsync(new CommandDefinition(insertTagSql, tag, transaction, cancellationToken: ct));

            var insertTaskWithTags = @"Insert into Tasks (Id,Title,Description,IsCompleted,CreatedAt,UserId) Values (@Id,@Title,@Description, @IsCompleted,@CreatedAt,@UserId)";
            var insertedTaskRows = await connection.ExecuteAsync(new CommandDefinition(insertTaskWithTags, task, transaction, cancellationToken: ct));

            var taskTag = new TaskTag { TaskId = task.Id, TagId = tag.Id };
            var insertTaskTagSql = @"Insert into TaskTags (TaskId,TagId) Values (@TaskId, @TagId)";

            var insertedRowsTasktag = await connection.ExecuteAsync(new CommandDefinition(insertTaskTagSql, taskTag, transaction, cancellationToken: ct));


            transaction.Commit();
            return task;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
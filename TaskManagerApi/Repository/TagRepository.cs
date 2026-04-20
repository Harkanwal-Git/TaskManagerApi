using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _dbContext;

    public TagRepository(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }
    public async Task<Tag> AddTag(Tag tag, CancellationToken ct)
    {
        _dbContext.Tags.Add(tag);

        await _dbContext.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<bool> DeleteTag(Guid tagId, CancellationToken ct)
    {
        var rowsDeleted = await _dbContext.Tags.Where(t => t.Id == tagId).ExecuteDeleteAsync(ct);
        return rowsDeleted > 0;
    }

    public async Task<IEnumerable<Tag>> GetTags(CancellationToken ct)
    {
        return await _dbContext.Tags.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IEnumerable<Tag>> GetTagsForTask(Guid taskId, CancellationToken ct)
    {
        return await _dbContext.TaskTags.AsNoTracking().Where(tt => tt.TaskId == taskId).Select(tt => tt.Tag).ToListAsync(ct);
    }
    public async Task<Tag?> GetTagById(Guid tagId, CancellationToken ct)
    {
        return await _dbContext.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId, ct);
    }
}
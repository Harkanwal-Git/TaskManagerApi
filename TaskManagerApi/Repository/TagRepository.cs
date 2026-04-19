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
    public async Task<Tag> AddTag(Tag tag)
    {
        _dbContext.Tags.Add(tag);

        await _dbContext.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteTag(Guid tagId)
    {
        var rowsDeleted = await _dbContext.Tags.Where(t => t.Id == tagId).ExecuteDeleteAsync();
        return rowsDeleted > 0;
    }

    public async Task<IEnumerable<Tag>> GetTags()
    {
        return await _dbContext.Tags.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Tag>> GetTagsForTask(Guid taskId)
    {
        return await _dbContext.TaskTags.AsNoTracking().Where(tt => tt.TaskId == taskId).Select(tt => tt.Tag).ToListAsync();
    }
    public async Task<Tag?> GetTagById(Guid tagId)
    {
        return await _dbContext.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId);
    }
}
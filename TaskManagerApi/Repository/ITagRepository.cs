using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface ITagRepository
{
    public Task<IEnumerable<Tag>> GetTags();
    public Task<IEnumerable<Tag>> GetTagsForTask(Guid taskId);
    public Task<Tag> AddTag(Tag tag);
    public Task<bool> DeleteTag(Guid TagId);
    public Task<Tag?> GetTagById(Guid tagId);
}
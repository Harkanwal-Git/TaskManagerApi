using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface ITagRepository
{
    public Task<IEnumerable<Tag>> GetTags(CancellationToken ct);
    public Task<IEnumerable<Tag>> GetTagsForTask(Guid taskId, CancellationToken ct);
    public Task<Tag> AddTag(Tag tag, CancellationToken ct);
    public Task<bool> DeleteTag(Guid TagId, CancellationToken ct);
    public Task<Tag?> GetTagById(Guid tagId, CancellationToken ct);
}
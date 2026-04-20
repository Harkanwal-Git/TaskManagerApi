using TaskManagerApi.DTO;
using TaskManagerApi.Model;

namespace TaskManagerApi.Service;

public interface ITagService
{
    public Task<ResponseTagDto> AddTag(string tagName, CancellationToken ct);

    public Task<bool> DeleteTag(Guid tagId, CancellationToken ct);

    public Task<IEnumerable<ResponseTagDto>> GetTags(CancellationToken ct);
    public Task<IEnumerable<ResponseTagDto>> GetTagsForTask(Guid taskId, CancellationToken ct);
}
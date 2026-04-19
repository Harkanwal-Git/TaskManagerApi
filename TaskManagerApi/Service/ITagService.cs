using TaskManagerApi.DTO;
using TaskManagerApi.Model;

namespace TaskManagerApi.Service;

public interface ITagService
{
    public Task<ResponseTagDto> AddTag(string tagName);

    public Task<bool> DeleteTag(Guid tagId);

    public Task<IEnumerable<ResponseTagDto>> GetTags();
    public Task<IEnumerable<ResponseTagDto>> GetTagsForTask(Guid taskId);
}
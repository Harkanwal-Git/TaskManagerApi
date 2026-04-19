using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Service;


public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;

    }
    public async Task<ResponseTagDto> AddTag(string tagName)
    {
        var tag = await _tagRepository.AddTag(MapTagNameToTagModel(tagName));

        return MapTagToResponseTagDto(tag);
    }

    public async Task<bool> DeleteTag(Guid tagId)
    {
        return await _tagRepository.DeleteTag(tagId);
    }

    public async Task<IEnumerable<ResponseTagDto>> GetTags()
    {
        var tags = await _tagRepository.GetTags();
        return tags.Select(t => MapTagToResponseTagDto(t));
    }

    public async Task<IEnumerable<ResponseTagDto>> GetTagsForTask(Guid taskId)
    {
        var tags = await _tagRepository.GetTagsForTask(taskId);
        return tags.Select(t => MapTagToResponseTagDto(t));
    }

    private Tag MapTagNameToTagModel(string tagName)
    {
        return new Tag { TagName = tagName.ToLower() };
    }

    private ResponseTagDto MapTagToResponseTagDto(Tag tag)
    {
        return new ResponseTagDto(TagId: tag.Id, TagName: tag.TagName);
    }
}
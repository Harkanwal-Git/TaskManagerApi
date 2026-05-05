using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Service;


public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ICacheRepository _cacheRepository;
    private const string TagsCacheKey = "tags";
    public TagService(ITagRepository tagRepository, ICacheRepository cacheRepository)
    {
        _tagRepository = tagRepository;
        _cacheRepository = cacheRepository;

    }
    public async Task<ResponseTagDto> AddTag(string tagName, CancellationToken ct)
    {
        var tag = await _tagRepository.AddTag(MapTagNameToTagModel(tagName), ct);
        await _cacheRepository.InvalidateCacheKey(TagsCacheKey, ct);
        return MapTagToResponseTagDto(tag);
    }

    public async Task<bool> DeleteTag(Guid tagId, CancellationToken ct)
    {
        var result = await _tagRepository.DeleteTag(tagId, ct);
        if (result) await _cacheRepository.InvalidateCacheKey(TagsCacheKey, ct);
        return result;
    }

    public async Task<IEnumerable<ResponseTagDto>> GetTags(CancellationToken ct)
    {
        // var cached = _cacheRepository.Get<IEnumerable<ResponseTagDto>>(TagsCacheKey);
        // if (cached != null) return cached;
        // var tags = await _tagRepository.GetTags(ct);
        // var responseTags = tags.Select(t => MapTagToResponseTagDto(t));
        // _memoryCache.Set(TagsCacheKey, responseTags, TimeSpan.FromHours(24));
        // return responseTags;
        var value = await _cacheRepository.GetOrSet<IEnumerable<ResponseTagDto>>(TagsCacheKey,
        async ct =>
        {
            var tags = await _tagRepository.GetTags(ct);
            return tags.Select(t => MapTagToResponseTagDto(t));
        }
    , TimeSpan.FromHours(24), ct);

        return value;
    }

    public async Task<IEnumerable<ResponseTagDto>> GetTagsForTask(Guid taskId, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForTask(taskId, ct);
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
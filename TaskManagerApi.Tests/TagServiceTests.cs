using Moq;
using TaskManagerApi.DTO;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;
using TaskManagerApi.Service;


namespace TaskManagerApi.Tests;


public class TagServiceTests
{
    private readonly Mock<ITagRepository> _mockTagRepository;

    private readonly ITagService _tagService;
    private readonly Mock<ICacheRepository> _mockCacheRepository;
    public TagServiceTests()
    {
        _mockTagRepository = new();
        _mockCacheRepository = new();
        _mockCacheRepository.Setup(c => c.InvalidateCacheKey(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                   .Returns(Task.CompletedTask);
        _tagService = new TagService(_mockTagRepository.Object, _mockCacheRepository.Object);

    }

    [Fact]
    public async Task AddTag_WithValidInputs_ReturnsCreatedTag()
    {

        // Given
        Tag capturedTag = default!;
        CancellationToken capturedCT = default;
        _mockTagRepository.Setup(tr => tr.AddTag(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
                            .Callback<Tag, CancellationToken>((tag, ct) =>
                            {
                                capturedCT = ct;
                                capturedTag = tag;
                            })
                            .ReturnsAsync((Tag tag, CancellationToken ct) => tag);

        // When
        string tagName = "test-tag";

        var result = await _tagService.AddTag(tagName, CancellationToken.None);
        // Then

        Assert.Equal(tagName.ToLower(), result.TagName);
        Assert.Equal(tagName, capturedTag?.TagName);

        _mockTagRepository.Verify(tr => tr.AddTag(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTag_WithValidTagId_ReturnsTrue()
    {
        // Given
        Guid existingId = Guid.NewGuid();
        _mockTagRepository.Setup(tr => tr.DeleteTag(It.Is<Guid>(id => id == existingId), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

        // When
        var result = await _tagService.DeleteTag(existingId, CancellationToken.None);
        // Then
        Assert.True(result);
        _mockTagRepository.Verify(tr => tr.DeleteTag(It.Is<Guid>(id => id == existingId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTag_WithInValidTagId_ReturnsFalse()
    {
        // Given
        Guid existingId = Guid.NewGuid();
        _mockTagRepository.Setup(tr => tr.DeleteTag(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);

        // When
        var result = await _tagService.DeleteTag(existingId, CancellationToken.None);
        // Then
        Assert.False(result);
        _mockTagRepository.Verify(tr => tr.DeleteTag(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheRepository.Verify(c => c.InvalidateCacheKey(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTags_CacheMiss_WhenEmpty_ReturnsEmptyIEnumerable()
    {
        // Given
        _mockTagRepository.Setup(tr => tr.GetTags(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Tag>());
        _mockCacheRepository
    .Setup(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
        It.IsAny<TimeSpan>(),
        It.IsAny<CancellationToken>()))
        .Returns<string, Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>, TimeSpan, CancellationToken>(async (key, factory, expiration, ct) => await factory(ct));
        // When
        var result = await _tagService.GetTags(CancellationToken.None);
        // Then
        Assert.IsAssignableFrom<IEnumerable<ResponseTagDto>>(result);
        Assert.Empty(result);

        _mockTagRepository.Verify(tr => tr.GetTags(It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheRepository.Verify(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
        It.IsAny<TimeSpan>(),
        It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetTags_CacheMiss_ReturnsTagsList()
    {
        // Given
        _mockTagRepository.Setup(tr => tr.GetTags(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Tag> { new Tag { TagName = "test tag1" }, new Tag { TagName = "test tag2" } });
        _mockCacheRepository
     .Setup(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
         It.IsAny<string>(),
         It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
         It.IsAny<TimeSpan>(),
         It.IsAny<CancellationToken>()))
         .Returns<string, Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>, TimeSpan, CancellationToken>(async (key, factory, expiration, ct) => await factory(ct));
        // When
        var result = await _tagService.GetTags(CancellationToken.None);
        // Then
        Assert.IsAssignableFrom<IEnumerable<ResponseTagDto>>(result);
        Assert.NotEmpty(result);

        _mockTagRepository.Verify(tr => tr.GetTags(It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheRepository.Verify(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
         It.IsAny<string>(),
         It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
         It.IsAny<TimeSpan>(),
         It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetTags_CacheHit_ReturnsTagsList()
    {
        // Given
        _mockTagRepository.Setup(tr => tr.GetTags(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Tag> { new Tag { TagName = "test tag1" }, new Tag { TagName = "test tag2" } });
        _mockCacheRepository.Setup(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
                            It.IsAny<string>(),
                            It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
                            It.IsAny<TimeSpan>(),
                            It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<ResponseTagDto> { new ResponseTagDto(TagId: Guid.NewGuid(), TagName: "test-tag") });
        // When
        var result = await _tagService.GetTags(CancellationToken.None);
        // Then
        Assert.IsAssignableFrom<IEnumerable<ResponseTagDto>>(result);
        Assert.NotEmpty(result);

        _mockTagRepository.Verify(tr => tr.GetTags(It.IsAny<CancellationToken>()), Times.Never);
        _mockCacheRepository.Verify(c => c.GetOrSet<IEnumerable<ResponseTagDto>>(
                            It.IsAny<string>(),
                            It.IsAny<Func<CancellationToken, Task<IEnumerable<ResponseTagDto>>>>(),
                            It.IsAny<TimeSpan>(),
                            It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetTagsForTask_WhenTaskTagsExists_ReturnsTagsForTask()
    {
        // Given
        _mockTagRepository.Setup(tr => tr.GetTagsForTask(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Tag> { new Tag { TagName = "test tag1" }, new Tag { TagName = "test tag2" } });
        // When
        var result = await _tagService.GetTagsForTask(Guid.NewGuid(), CancellationToken.None);
        // Then
        Assert.IsAssignableFrom<IEnumerable<ResponseTagDto>>(result);
        Assert.NotEmpty(result);

        _mockTagRepository.Verify(tr => tr.GetTagsForTask(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTagsForTask_WhenTaskTagsDontExists_ReturnsEmptyList()
    {
        // Given
        _mockTagRepository.Setup(tr => tr.GetTagsForTask(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Tag>());
        // When
        var result = await _tagService.GetTagsForTask(Guid.NewGuid(), CancellationToken.None);
        // Then
        Assert.IsAssignableFrom<IEnumerable<ResponseTagDto>>(result);
        Assert.Empty(result);

        _mockTagRepository.Verify(tr => tr.GetTagsForTask(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
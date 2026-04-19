namespace TaskManagerApi.Model;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string TagName { get; set; }

    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
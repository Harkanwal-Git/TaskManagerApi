namespace TaskManagerApi.Model;


public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;  //navigation prop
    public Tag Tag { get; set; } = null!;       //navigation prop

}
namespace TaskManagerApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IdempotentAttribute : Attribute
{
    public string HeaderName { get; }

    public IdempotentAttribute(string headerName = "Idempotent-Key")
    {
        this.HeaderName = headerName;
    }
}
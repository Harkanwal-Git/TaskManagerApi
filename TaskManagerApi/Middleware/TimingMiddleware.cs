namespace TaskManagerApi.Middleware;

public class TimingMiddleware
{
    private readonly RequestDelegate _next;
    public TimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        await _next(context);
        var duration = DateTime.UtcNow - start;
        System.Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path} took : {duration.TotalMilliseconds:F2} ms");
    }
}

public static class TimingMiddlewareExtensions
{
    public static IApplicationBuilder UseTimingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TimingMiddleware>();
    }
}
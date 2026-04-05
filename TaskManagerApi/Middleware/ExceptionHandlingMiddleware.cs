using Microsoft.AspNetCore.Mvc;

namespace TaskManagerApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {

        (int statusCode, string errorTitle) = MapException(exception);
        httpContext.Response.StatusCode = statusCode;


        var result = new ProblemDetails { Status = statusCode, Title = errorTitle, Detail = "An error occurred", Instance = httpContext.Request.Path };
        result.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(result);
    }
    private static (int statusCode, string title) MapException(Exception ex)
    {
        return ex switch
        {
            ArgumentException => (400, "Bad Request"),
            UnauthorizedAccessException => (401, "UnAuthorrized"),
            KeyNotFoundException => (404, "Not Found"),
            _ => (500, "Internal Server Error")
        };
    }
}

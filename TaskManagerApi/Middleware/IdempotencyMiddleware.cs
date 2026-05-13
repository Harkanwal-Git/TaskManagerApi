using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Attributes;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheRepository _cache;

    public IdempotencyMiddleware(RequestDelegate next, ICacheRepository cacheRepository)
    {
        _next = next;
        _cache = cacheRepository;
    }
    public async Task InvokeAsync(HttpContext httpContext, IIdempotencyRecordRepository idempotencyRecordRepository)
    {
        // if (httpContext.Request.Method != HttpMethods.Post) { await _next(httpContext); return; }

        var endpoint = httpContext.GetEndpoint();
        var idempotentAttribute = endpoint?.Metadata.GetMetadata<IdempotentAttribute>();
        if (idempotentAttribute == null) { await _next(httpContext); return; }

        httpContext.Request.EnableBuffering();
        if (!Guid.TryParse(httpContext.Request.Headers[idempotentAttribute.HeaderName], out var idempotencyKey)) //throw new BadHttpRequestException("Missing Idempotency header");
        {
            await _next(httpContext);
            return;
        }

        //  var result = await _cache.GetLazyWay($"Idempotency:{idempotencyKey}", (ct) => _idempotencyRecordRepository.GetByKey(idempotencyKey, httpContext.RequestAborted));

        var result = await _cache.Get<IdempotencyRecord>($"Idempotency:{idempotencyKey}", httpContext.RequestAborted);

        if (result == null)
        {
            result = await idempotencyRecordRepository.GetByKey(idempotencyKey, httpContext.RequestAborted);
            if (result != null)
            {
                await _cache.Set($"Idempotency:{idempotencyKey}", result, TimeSpan.FromHours(24), httpContext.RequestAborted);
            }
        }
        var requestPayload = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        httpContext.Request.Body.Position = 0;

        if (result != null)
        {
            if (requestPayload != result?.RequestPayload)
            {
                httpContext.Response.StatusCode = 422;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = 422,
                    Title = "Idempotency conflict",
                    Detail = "Same key used with different payload"
                });
                return;
            }
            httpContext.Response.StatusCode = result.StatusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(result.ResponsePayload);
            return;
        }

        //capture response

        var originalBody = httpContext.Response.Body;

        using var memStream = new MemoryStream();
        httpContext.Response.Body = memStream;
        await _next(httpContext);

        memStream.Position = 0;

        var responseBody = await new StreamReader(memStream).ReadToEndAsync();
        var record = new IdempotencyRecord() { IdempotencyKey = idempotencyKey, RequestPayload = requestPayload, ResponsePayload = responseBody, StatusCode = httpContext.Response.StatusCode, ExpiresAt = DateTime.UtcNow.AddHours(24) };

        await idempotencyRecordRepository.Add(record, httpContext.RequestAborted);

        await _cache.Set($"Idempotency:{idempotencyKey}", record, TimeSpan.FromHours(24), httpContext.RequestAborted);

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);

        httpContext.Response.Body = originalBody;
    }
}

public static class UseIdempotencyExtension
{
    public static IApplicationBuilder UseIdempotencyMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
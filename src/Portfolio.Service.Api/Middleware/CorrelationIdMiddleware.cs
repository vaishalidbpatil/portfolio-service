using Portfolio.Service.Configuration;
using Serilog.Context;

namespace Portfolio.Service.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, CorrelationContext correlation)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault()
                 ?? Guid.NewGuid().ToString("N");

        correlation.CorrelationId = id;
        context.Response.Headers[HeaderName] = id;

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await _next(context);
        }
    }
}

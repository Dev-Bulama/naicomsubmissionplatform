using Serilog.Context;

namespace NLIP.API.Middleware;

/// <summary>Every request gets a Correlation ID (reused from the X-Correlation-Id request header
/// when the caller — e.g. the Core Application connector — already supplies one), pushed into
/// both the response header and the Serilog LogContext so it threads through every log line and
/// into NaicomTransaction/ApiCallLog for a request's whole lifecycle.</summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

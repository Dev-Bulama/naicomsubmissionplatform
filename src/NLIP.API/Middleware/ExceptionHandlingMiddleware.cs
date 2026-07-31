using System.Net;
using System.Text.Json;
using NLIP.Application.Common.Exceptions;
using ValidationException = NLIP.Application.Common.Exceptions.ValidationException;

namespace NLIP.API.Middleware;

/// <summary>
/// Central place turning every exception into the user-friendly error shapes the spec calls for
/// (Authentication Failed, Validation Failed, Policy Already Exists/Not Found, Timeout,
/// Connection Failure, NAICOM Unavailable, Unknown Error) instead of leaking stack traces.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationEx => (HttpStatusCode.BadRequest, "Validation Failed", (object?)validationEx.Errors),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message, null),
            NaicomApiException naicomEx => (HttpStatusCode.BadGateway, $"NAICOM Unavailable: {naicomEx.Message}", null),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "Timeout", null),
            _ => (HttpStatusCode.InternalServerError, "Unknown Error", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "NLIP Unhandled exception on {Path}", context.Request.Path);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            correlationId = context.Items.TryGetValue("CorrelationId", out var cid) ? cid?.ToString() : null,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

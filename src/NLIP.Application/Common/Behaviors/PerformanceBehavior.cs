using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NLIP.Application.Common.Behaviors;

/// <summary>Logs a warning for any request/command taking longer than 500ms — cheap, always-on
/// slow-query detection without needing full APM tooling.</summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning("NLIP Slow Request: {RequestName} took {ElapsedMs}ms",
                typeof(TRequest).Name, sw.ElapsedMilliseconds);
        }

        return response;
    }
}

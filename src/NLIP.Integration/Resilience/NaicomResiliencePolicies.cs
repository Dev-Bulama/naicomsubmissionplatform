using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace NLIP.Integration.Resilience;

/// <summary>
/// Composes the four Polly policies required for every outbound call to NAICOM: Retry (jittered
/// exponential backoff so transient failures aren't hammered), Circuit Breaker (stop calling a
/// visibly-down NAICOM instead of piling up timeouts), Timeout (bound how long a single attempt
/// waits), and Bulkhead (cap concurrent NAICOM calls so a slow NAICOM can't exhaust the app's
/// thread/connection pool). Registered on the typed HttpClient via AddPolicyHandler in
/// DependencyInjection.cs. A Fallback is deliberately NOT applied here — a failed HTTP call should
/// surface as a failed NaicomApiResponse so the outbox retry engine (not Polly) owns the "what do
/// we do next" business decision.
/// </summary>
public static class NaicomResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> Build(Client.NaicomOptions options, ILogger logger)
    {
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), options.RetryCount),
                onRetry: (outcome, delay, attempt, _) =>
                    logger.LogWarning("NLIP NAICOM retry {Attempt}/{Max} after {Delay}ms: {Reason}",
                        attempt, options.RetryCount, delay.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                options.CircuitBreakerFailureThreshold,
                TimeSpan.FromSeconds(options.CircuitBreakerBreakSeconds),
                onBreak: (outcome, breakDelay) => logger.LogError(
                    "NLIP NAICOM circuit OPEN for {BreakSeconds}s after repeated failures: {Reason}",
                    breakDelay.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()),
                onReset: () => logger.LogInformation("NLIP NAICOM circuit CLOSED — calls resumed"),
                onHalfOpen: () => logger.LogInformation("NLIP NAICOM circuit HALF-OPEN — testing NAICOM availability"));

        var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(options.TimeoutSeconds), TimeoutStrategy.Optimistic);

        var bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(
            options.BulkheadMaxParallelization, options.BulkheadMaxQueuedActions,
            onBulkheadRejectedAsync: _ =>
            {
                logger.LogWarning("NLIP NAICOM bulkhead rejected a request — too many concurrent NAICOM calls in flight");
                return Task.CompletedTask;
            });

        // Order matters: bulkhead outermost (reject fast if saturated) -> circuit breaker (fail
        // fast if NAICOM is down) -> retry (only retry a call that got through the breaker) ->
        // timeout innermost (bounds each individual attempt, including each retry).
        return Policy.WrapAsync(bulkhead, circuitBreaker, retry, timeout);
    }
}

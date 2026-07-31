using System.Linq.Expressions;
using Hangfire;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Jobs;

/// <summary>
/// Adapter over Hangfire's client API (IBackgroundJobClient/IRecurringJobManager), which both the
/// API (enqueue-only — e.g. manual Retry button) and the Worker (enqueue + actually run jobs,
/// since it also hosts a BackgroundJobServer) register against the same Hangfire SQL Server
/// storage, giving one shared queue/dashboard across both processes.
/// </summary>
public class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _client;
    private readonly IRecurringJobManager _recurring;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient client, IRecurringJobManager recurring)
    {
        _client = client;
        _recurring = recurring;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => _client.Enqueue(methodCall);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => _client.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
        => _recurring.AddOrUpdate(recurringJobId, methodCall, cronExpression);
}

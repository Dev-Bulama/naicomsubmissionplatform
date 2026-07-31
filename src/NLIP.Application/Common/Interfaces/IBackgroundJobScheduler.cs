using System.Linq.Expressions;

namespace NLIP.Application.Common.Interfaces;

/// <summary>
/// Thin abstraction over the background job engine (Hangfire, implemented in NLIP.Worker /
/// NLIP.Infrastructure) so Application code can enqueue/schedule work without depending on the
/// Hangfire package directly.
/// </summary>
public interface IBackgroundJobScheduler
{
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    void AddOrUpdateRecurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);
}

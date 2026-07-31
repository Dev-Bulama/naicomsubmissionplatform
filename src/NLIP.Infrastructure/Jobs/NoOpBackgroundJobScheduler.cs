using System.Linq.Expressions;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Jobs;

/// <summary>Test-only stand-in for the Hangfire-backed scheduler (see AddInfrastructure's
/// useHangfire parameter) so integration tests can exercise MediatR handlers that enqueue jobs
/// without a real Hangfire/SQL Server dependency.</summary>
public class NoOpBackgroundJobScheduler : IBackgroundJobScheduler
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => Guid.NewGuid().ToString();
    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => Guid.NewGuid().ToString();
    public void AddOrUpdateRecurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression) { }
}

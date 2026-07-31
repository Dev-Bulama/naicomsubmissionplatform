using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Dashboard.Dtos;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly INaicomApiClient _naicomClient;
    private readonly ICacheService _cache;
    private readonly IDateTimeProvider _dateTime;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context, INaicomApiClient naicomClient, ICacheService cache, IDateTimeProvider dateTime)
    {
        _context = context;
        _naicomClient = naicomClient;
        _cache = cache;
        _dateTime = dateTime;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync("dashboard:stats", () => BuildAsync(cancellationToken), TimeSpan.FromSeconds(30), cancellationToken);
    }

    private async Task<DashboardStatsDto> BuildAsync(CancellationToken cancellationToken)
    {
        var today = _dateTime.UtcNow.Date;
        var last30Days = today.AddDays(-30);

        var totalPolicies = await _context.Policies.CountAsync(cancellationToken);
        var retail = await _context.Policies.CountAsync(p => p.BusinessType == BusinessType.IndividualLife, cancellationToken);
        var group = await _context.Policies.CountAsync(p => p.BusinessType == BusinessType.GroupLife, cancellationToken);

        var submittedToday = await _context.NaicomTransactions.CountAsync(t => t.AttemptedAt.Date == today, cancellationToken);
        var successful = await _context.NaicomTransactions.CountAsync(t => t.IsSuccessful, cancellationToken);
        var failed = await _context.NaicomTransactions.CountAsync(t => !t.IsSuccessful, cancellationToken);
        var pending = await _context.SubmissionQueue.CountAsync(s => s.Status == SubmissionStatus.Queued, cancellationToken);
        var retrying = await _context.SubmissionQueue.CountAsync(s => s.Status == SubmissionStatus.Retrying, cancellationToken);
        var processing = await _context.SubmissionQueue.CountAsync(s => s.Status == SubmissionStatus.Processing, cancellationToken);

        var total = successful + failed;
        var avgResponseTime = await _context.NaicomTransactions.AnyAsync(cancellationToken)
            ? await _context.NaicomTransactions.AverageAsync(t => (double)t.DurationMs, cancellationToken)
            : 0d;

        var dailyVolume = await _context.NaicomTransactions
            .Where(t => t.AttemptedAt >= last30Days)
            .GroupBy(t => t.AttemptedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(cancellationToken);

        var topErrors = await _context.NaicomTransactions
            .Where(t => !t.IsSuccessful && t.ErrorMessage != null)
            .GroupBy(t => t.ErrorMessage)
            .Select(g => new TopErrorDto { ErrorMessage = g.Key!, Count = g.Count() })
            .OrderByDescending(e => e.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentActivities = await _context.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new RecentActivityDto { Description = a.Description, Category = a.Category, Timestamp = a.Timestamp })
            .ToListAsync(cancellationToken);

        NaicomHealthCheck? health = null;
        try
        {
            var status = await _naicomClient.CheckHealthAsync(cancellationToken);
            health = new NaicomHealthCheck(status.IsAvailable, status.Version);
        }
        catch
        {
            health = new NaicomHealthCheck(false, null);
        }

        return new DashboardStatsDto
        {
            TotalPolicies = totalPolicies,
            PoliciesSubmittedToday = submittedToday,
            RetailPolicies = retail,
            GroupPolicies = group,
            SuccessfulSubmissions = successful,
            FailedSubmissions = failed,
            PendingSubmissions = pending,
            RetryQueueCount = retrying,
            ProcessingQueueCount = processing,
            AverageResponseTimeMs = Math.Round(avgResponseTime, 2),
            SuccessRatePercent = total == 0 ? 100 : Math.Round(successful * 100.0 / total, 2),
            FailureRatePercent = total == 0 ? 0 : Math.Round(failed * 100.0 / total, 2),
            DailyVolume = dailyVolume.Select(d => new VolumePointDto { Label = d.Date.ToString("yyyy-MM-dd"), Count = d.Count }).ToList(),
            NaicomApiAvailable = health.IsAvailable,
            NaicomAuthenticated = health.IsAvailable,
            NaicomVersion = health.Version,
            TopErrors = topErrors,
            RecentActivities = recentActivities
        };
    }

    private record NaicomHealthCheck(bool IsAvailable, string? Version);
}

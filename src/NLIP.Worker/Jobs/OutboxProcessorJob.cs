using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Submissions;
using NLIP.Domain.Enums;

namespace NLIP.Worker.Jobs;

/// <summary>
/// The heart of the retry/outbox engine. Runs on a short recurring schedule (see
/// Program.cs — every 15s) and picks up every SubmissionQueue row that is due (Queued or
/// Retrying, NextAttemptAt elapsed) and not already scheduled (HangfireJobId is null), enqueuing
/// one Hangfire job per row. Deliberately does the actual NAICOM call in a separate job
/// (ProcessSubmissionCommandDispatcher) rather than inline here, so a slow/hanging NAICOM call
/// can't block the poller from picking up the next batch.
/// </summary>
public class OutboxProcessorJob
{
    private readonly IApplicationDbContext _context;
    private readonly IBackgroundJobScheduler _scheduler;
    private readonly ILogger<OutboxProcessorJob> _logger;

    public OutboxProcessorJob(IApplicationDbContext context, IBackgroundJobScheduler scheduler, ILogger<OutboxProcessorJob> logger)
    {
        _context = context;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var due = await _context.SubmissionQueue
            .Where(s => (s.Status == SubmissionStatus.Queued || s.Status == SubmissionStatus.Retrying)
                        && s.HangfireJobId == null
                        && (s.NextAttemptAt == null || s.NextAttemptAt <= now))
            .OrderBy(s => s.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        if (due.Count == 0) return;

        foreach (var submission in due)
        {
            var jobId = _scheduler.Enqueue<ProcessSubmissionCommandDispatcher>(d => d.DispatchAsync(submission.Id));
            submission.HangfireJobId = jobId;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("NLIP Outbox: enqueued {Count} submission(s) for processing", due.Count);
    }
}

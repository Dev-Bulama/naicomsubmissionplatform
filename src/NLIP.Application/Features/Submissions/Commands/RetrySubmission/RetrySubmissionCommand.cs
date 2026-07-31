using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Submissions.Commands.ProcessSubmission;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Submissions.Commands.RetrySubmission;

/// <summary>Manual "Retry" button on the Sync Monitor / Policy Detail screens — immediately
/// re-queues a Failed/DeadLetter/Retrying submission instead of waiting for its scheduled NextAttemptAt.</summary>
public record RetrySubmissionCommand(Guid SubmissionQueueId) : IRequest;

public class RetrySubmissionCommandHandler : IRequestHandler<RetrySubmissionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IBackgroundJobScheduler _scheduler;

    public RetrySubmissionCommandHandler(IApplicationDbContext context, IBackgroundJobScheduler scheduler)
    {
        _context = context;
        _scheduler = scheduler;
    }

    public async Task Handle(RetrySubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.SubmissionQueue.FirstOrDefaultAsync(s => s.Id == request.SubmissionQueueId, cancellationToken)
            ?? throw new NotFoundException("SubmissionQueue", request.SubmissionQueueId);

        submission.Status = SubmissionStatus.Queued;
        submission.NextAttemptAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _scheduler.Enqueue<ProcessSubmissionCommandDispatcher>(d => d.DispatchAsync(submission.Id));
    }
}

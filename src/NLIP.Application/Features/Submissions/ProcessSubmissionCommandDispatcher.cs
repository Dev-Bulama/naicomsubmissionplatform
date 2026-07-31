using MediatR;
using NLIP.Application.Features.Submissions.Commands.ProcessSubmission;

namespace NLIP.Application.Features.Submissions;

/// <summary>
/// Hangfire invokes this class by name via reflection (see IBackgroundJobScheduler), so job
/// arguments serialized into Hangfire's SQL Server storage stay as a single Guid rather than a
/// full MediatR command payload. It just forwards to the MediatR pipeline (validation, logging,
/// exception behaviors all still apply).
/// </summary>
public class ProcessSubmissionCommandDispatcher
{
    private readonly IMediator _mediator;

    public ProcessSubmissionCommandDispatcher(IMediator mediator) => _mediator = mediator;

    public Task DispatchAsync(Guid submissionQueueId) => _mediator.Send(new ProcessSubmissionCommand(submissionQueueId));
}

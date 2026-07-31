using MediatR;

namespace NLIP.Application.Features.Submissions.Commands.ProcessSubmission;

/// <summary>
/// Invoked by the Hangfire outbox processor (and by manual Retry/Resubmit from the UI) for a
/// single SubmissionQueue row. Calls NAICOM through INaicomApiClient, records the transaction,
/// updates the policy, and — on failure — reschedules itself according to
/// SubmissionQueue.RetryScheduleSeconds up to MaxRetries, after which the row is marked DeadLetter
/// and a notification is broadcast to Integration Administrators.
/// </summary>
public record ProcessSubmissionCommand(Guid SubmissionQueueId) : IRequest;

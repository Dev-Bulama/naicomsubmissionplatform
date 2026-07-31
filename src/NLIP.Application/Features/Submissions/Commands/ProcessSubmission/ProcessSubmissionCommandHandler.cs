using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Common.Models;
using NLIP.Domain.Entities.Integration;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Submissions.Commands.ProcessSubmission;

public class ProcessSubmissionCommandHandler : IRequestHandler<ProcessSubmissionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INaicomApiClient _naicomClient;
    private readonly INotificationService _notifications;
    private readonly ISettingsService _settings;
    private readonly ILogger<ProcessSubmissionCommandHandler> _logger;

    public ProcessSubmissionCommandHandler(
        IApplicationDbContext context,
        INaicomApiClient naicomClient,
        INotificationService notifications,
        ISettingsService settings,
        ILogger<ProcessSubmissionCommandHandler> logger)
    {
        _context = context;
        _naicomClient = naicomClient;
        _notifications = notifications;
        _settings = settings;
        _logger = logger;
    }

    public async Task Handle(ProcessSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.SubmissionQueue.FirstOrDefaultAsync(s => s.Id == request.SubmissionQueueId, cancellationToken);
        if (submission is null || submission.Status is SubmissionStatus.Completed or SubmissionStatus.Cancelled or SubmissionStatus.DeadLetter)
            return;

        var policy = await _context.Policies
            .Include(p => p.Beneficiaries)
            .Include(p => p.GroupMembers)
            .FirstOrDefaultAsync(p => p.Id == submission.PolicyId, cancellationToken);

        if (policy is null)
        {
            submission.Status = SubmissionStatus.Cancelled;
            submission.LastError = "Policy no longer exists.";
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        submission.Status = SubmissionStatus.Processing;
        submission.LastAttemptAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var response = await CallNaicomAsync(submission.Action, policy, cancellationToken);

        _context.NaicomTransactions.Add(new NaicomTransaction
        {
            PolicyId = policy.Id,
            Action = submission.Action,
            NaicomPolicyId = response.NaicomPolicyId ?? policy.NaicomPolicyId,
            CorrelationId = response.CorrelationId,
            RequestPayload = submission.Payload,
            ResponsePayload = response.RawResponse,
            HttpStatusCode = response.HttpStatusCode,
            IsSuccessful = response.IsSuccessful,
            ErrorMessage = response.ErrorMessage,
            DurationMs = response.DurationMs
        });

        if (response.IsSuccessful)
        {
            if (!string.IsNullOrWhiteSpace(response.NaicomPolicyId))
                policy.SetNaicomPolicyId(response.NaicomPolicyId);

            policy.MarkSubmissionResult(SubmissionStatus.Completed, incrementRetry: false);
            submission.Status = SubmissionStatus.Completed;
            submission.ProcessedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notifications.BroadcastAsync(
                "NAICOM Submission Succeeded",
                $"Policy {policy.PolicyNumber} ({submission.Action}) submitted successfully. NAICOM ID: {policy.NaicomPolicyId}",
                NotificationChannel.SignalR, policy.Id, cancellationToken);
        }
        else
        {
            var maxRetries = await _settings.GetIntAsync(Shared.Constants.SettingKeys.RetryMaxAttempts, submission.MaxRetries, cancellationToken);
            submission.RetryCount++;
            submission.LastError = response.ErrorMessage;

            if (submission.RetryCount >= maxRetries)
            {
                submission.Status = SubmissionStatus.DeadLetter;
                policy.MarkSubmissionResult(SubmissionStatus.Failed, incrementRetry: true);

                await _notifications.BroadcastAsync(
                    "NAICOM Submission Failed — Dead Letter",
                    $"Policy {policy.PolicyNumber} ({submission.Action}) exhausted {maxRetries} retries: {response.ErrorMessage}",
                    NotificationChannel.SignalR, policy.Id, cancellationToken);
            }
            else
            {
                var scheduleIndex = Math.Min(submission.RetryCount - 1, Domain.Entities.Integration.SubmissionQueue.RetryScheduleSeconds.Length - 1);
                var delaySeconds = Domain.Entities.Integration.SubmissionQueue.RetryScheduleSeconds[scheduleIndex];
                submission.Status = SubmissionStatus.Retrying;
                submission.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
                // Clear so the outbox poller (NLIP.Worker.Jobs.OutboxProcessorJob) re-enqueues a
                // fresh Hangfire job once NextAttemptAt elapses, instead of assuming the
                // already-completed job id is still "in flight".
                submission.HangfireJobId = null;
                policy.MarkSubmissionResult(SubmissionStatus.Retrying, incrementRetry: true);

                _logger.LogWarning("NLIP Submission {SubmissionId} failed (attempt {Attempt}); retrying in {Delay}s: {Error}",
                    submission.Id, submission.RetryCount, delaySeconds, response.ErrorMessage);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private Task<NaicomApiResponse> CallNaicomAsync(NaicomAction action, Domain.Entities.Policies.Policy policy, CancellationToken cancellationToken) => action switch
    {
        NaicomAction.Create => _naicomClient.CreatePolicyAsync(policy, cancellationToken),
        NaicomAction.Update => _naicomClient.UpdatePolicyAsync(policy, cancellationToken),
        NaicomAction.Renew => _naicomClient.RenewPolicyAsync(policy, cancellationToken),
        NaicomAction.Terminate => _naicomClient.TerminatePolicyAsync(policy, "Terminated via Core Application", cancellationToken),
        NaicomAction.Delete => _naicomClient.DeletePolicyAsync(policy.NaicomPolicyId ?? string.Empty, policy.BusinessType, cancellationToken),
        NaicomAction.Query => _naicomClient.QueryPolicyAsync(policy.NaicomPolicyId ?? string.Empty, policy.BusinessType, cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
}

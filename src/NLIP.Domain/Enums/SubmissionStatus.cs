namespace NLIP.Domain.Enums;

/// <summary>Status of a queued NAICOM submission as it moves through the outbox/retry engine.</summary>
public enum SubmissionStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Retrying = 4,
    DeadLetter = 5,
    Cancelled = 6
}

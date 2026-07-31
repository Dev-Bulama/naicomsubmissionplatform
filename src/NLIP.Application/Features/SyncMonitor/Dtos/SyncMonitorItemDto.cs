using NLIP.Domain.Enums;

namespace NLIP.Application.Features.SyncMonitor.Dtos;

public class SyncMonitorItemDto
{
    public Guid SubmissionId { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = default!;
    public BusinessType BusinessType { get; set; }
    public NaicomAction Action { get; set; }
    public SubmissionStatus Status { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public long? LastExecutionMs { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

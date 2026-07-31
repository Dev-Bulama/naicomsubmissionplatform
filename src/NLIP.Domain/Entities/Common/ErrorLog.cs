using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Common;

public class ErrorLog : BaseEntity
{
    public string Source { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? StackTrace { get; set; }
    public Guid? CorrelationId { get; set; }
    public string Severity { get; set; } = "Error";
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Resolved { get; set; }
}

using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Policies.Dtos;

public class PolicySummaryDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = default!;
    public string CorePolicyId { get; set; } = default!;
    public string? NaicomPolicyId { get; set; }
    public BusinessType BusinessType { get; set; }
    public PolicyStatus Status { get; set; }
    public SubmissionStatus CurrentSubmissionStatus { get; set; }
    public string? CustomerOrEmployerName { get; set; }
    public string? BranchName { get; set; }
    public string? AgentName { get; set; }
    public decimal SumAssured { get; set; }
    public decimal PremiumAmount { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? LastSubmittedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Policies.Dtos;

public class PolicyDetailDto : PolicySummaryDto
{
    public DateTime CoverageStartDate { get; set; }
    public DateTime CoverageEndDate { get; set; }
    public string PremiumFrequency { get; set; } = default!;

    public List<BeneficiaryDto> Beneficiaries { get; set; } = new();
    public List<GroupMemberDto> GroupMembers { get; set; } = new();
    public List<PolicyHistoryDto> History { get; set; } = new();
    public List<NaicomTransactionDto> Transactions { get; set; } = new();
    public List<SubmissionQueueDto> SubmissionTimeline { get; set; } = new();
}

public class BeneficiaryDto
{
    public string FullName { get; set; } = default!;
    public string Relationship { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public decimal SharePercentage { get; set; }
}

public class GroupMemberDto
{
    public string EmployeeId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default!;
    public decimal SumAssured { get; set; }
    public bool IsActive { get; set; }
}

public class PolicyHistoryDto
{
    public PolicyStatus PreviousStatus { get; set; }
    public PolicyStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

public class NaicomTransactionDto
{
    public Guid Id { get; set; }
    public NaicomAction Action { get; set; }
    public bool IsSuccessful { get; set; }
    public int? HttpStatusCode { get; set; }
    public string RequestPayload { get; set; } = default!;
    public string? ResponsePayload { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
}

public class SubmissionQueueDto
{
    public Guid Id { get; set; }
    public NaicomAction Action { get; set; }
    public SubmissionStatus Status { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

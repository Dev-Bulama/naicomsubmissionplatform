namespace NLIP.Web.Models;

// Thin client-side mirrors of the API's DTOs. NLIP.Web deliberately does not reference
// NLIP.Application/Domain/Persistence — it talks to NLIP.API purely over HTTP/JSON (and
// SignalR), the same way any external client would, so the UI stays swappable and the API
// remains the single authority over business rules.

public class AuthResult
{
    public string AccessToken { get; set; } = default!;
    public DateTimeOffset AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PolicySummary
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = default!;
    public string CorePolicyId { get; set; } = default!;
    public string? NaicomPolicyId { get; set; }
    public string BusinessType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string CurrentSubmissionStatus { get; set; } = default!;
    public string? CustomerOrEmployerName { get; set; }
    public string? BranchName { get; set; }
    public string? AgentName { get; set; }
    public decimal SumAssured { get; set; }
    public decimal PremiumAmount { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? LastSubmittedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PolicyDetail : PolicySummary
{
    public DateTime CoverageStartDate { get; set; }
    public DateTime CoverageEndDate { get; set; }
    public string PremiumFrequency { get; set; } = default!;
    public List<BeneficiaryItem> Beneficiaries { get; set; } = new();
    public List<GroupMemberItem> GroupMembers { get; set; } = new();
    public List<PolicyHistoryItem> History { get; set; } = new();
    public List<NaicomTransactionItem> Transactions { get; set; } = new();
    public List<SubmissionQueueItem> SubmissionTimeline { get; set; } = new();
}

public class BeneficiaryItem { public string FullName { get; set; } = default!; public string Relationship { get; set; } = default!; public decimal SharePercentage { get; set; } }
public class GroupMemberItem { public string EmployeeId { get; set; } = default!; public string FullName { get; set; } = default!; public decimal SumAssured { get; set; } public bool IsActive { get; set; } }
public class PolicyHistoryItem { public string PreviousStatus { get; set; } = default!; public string NewStatus { get; set; } = default!; public string? Notes { get; set; } public DateTimeOffset ChangedAt { get; set; } }
public class NaicomTransactionItem { public Guid Id { get; set; } public string Action { get; set; } = default!; public bool IsSuccessful { get; set; } public int? HttpStatusCode { get; set; } public string? ErrorMessage { get; set; } public long DurationMs { get; set; } public DateTimeOffset AttemptedAt { get; set; } }
public class SubmissionQueueItem { public Guid Id { get; set; } public string Action { get; set; } = default!; public string Status { get; set; } = default!; public int RetryCount { get; set; } public int MaxRetries { get; set; } public DateTimeOffset? NextAttemptAt { get; set; } public string? LastError { get; set; } public DateTimeOffset CreatedAt { get; set; } }

public class SyncMonitorItem
{
    public Guid SubmissionId { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = default!;
    public string BusinessType { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class DashboardStats
{
    public int TotalPolicies { get; set; }
    public int PoliciesSubmittedToday { get; set; }
    public int RetailPolicies { get; set; }
    public int GroupPolicies { get; set; }
    public int SuccessfulSubmissions { get; set; }
    public int FailedSubmissions { get; set; }
    public int PendingSubmissions { get; set; }
    public int RetryQueueCount { get; set; }
    public int ProcessingQueueCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double SuccessRatePercent { get; set; }
    public double FailureRatePercent { get; set; }
    public List<VolumePoint> DailyVolume { get; set; } = new();
    public bool NaicomApiAvailable { get; set; }
    public bool NaicomAuthenticated { get; set; }
    public string? NaicomVersion { get; set; }
    public List<TopError> TopErrors { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
}

public class VolumePoint { public string Label { get; set; } = default!; public int Count { get; set; } }
public class TopError { public string ErrorMessage { get; set; } = default!; public int Count { get; set; } }
public class RecentActivity { public string Description { get; set; } = default!; public string Category { get; set; } = default!; public DateTimeOffset Timestamp { get; set; } }

public class SettingItem { public string Key { get; set; } = default!; public string? Value { get; set; } public string? Description { get; set; } public bool IsSecret { get; set; } }

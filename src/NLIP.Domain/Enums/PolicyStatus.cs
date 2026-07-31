namespace NLIP.Domain.Enums;

/// <summary>Lifecycle status of a policy as tracked in this platform (independent of submission status).</summary>
public enum PolicyStatus
{
    Draft = 0,
    Active = 1,
    Updated = 2,
    Renewed = 3,
    Terminated = 4,
    Cancelled = 5
}

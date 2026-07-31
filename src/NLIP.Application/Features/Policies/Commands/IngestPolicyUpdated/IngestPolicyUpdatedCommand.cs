using MediatR;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyUpdated;

/// <summary>Raised by the Core Application Connector when policy data (sum assured, premium,
/// coverage end date) changes on an already-activated policy. Queues an Update (or, if the
/// policy has no NaicomPolicyId yet, a Create) submission.</summary>
public record IngestPolicyUpdatedCommand : IRequest
{
    public required string CorePolicyId { get; init; }
    public required decimal SumAssured { get; init; }
    public required decimal PremiumAmount { get; init; }
    public required DateTime CoverageEndDate { get; init; }
}

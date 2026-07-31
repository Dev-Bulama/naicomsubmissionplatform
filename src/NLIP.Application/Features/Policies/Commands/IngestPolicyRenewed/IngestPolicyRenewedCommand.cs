using MediatR;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyRenewed;

public record IngestPolicyRenewedCommand : IRequest
{
    public required string CorePolicyId { get; init; }
    public required DateTime NewCoverageEndDate { get; init; }
    public decimal? NewPremiumAmount { get; init; }
}

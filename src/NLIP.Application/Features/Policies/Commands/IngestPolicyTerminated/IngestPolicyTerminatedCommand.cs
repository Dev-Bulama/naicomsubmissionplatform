using MediatR;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyTerminated;

public record IngestPolicyTerminatedCommand : IRequest
{
    public required string CorePolicyId { get; init; }
    public required string Reason { get; init; }
}

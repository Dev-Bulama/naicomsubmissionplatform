using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyRenewed;

public class IngestPolicyRenewedCommandHandler : IRequestHandler<IngestPolicyRenewedCommand>
{
    private readonly IApplicationDbContext _context;

    public IngestPolicyRenewedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(IngestPolicyRenewedCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.Policies.FirstOrDefaultAsync(p => p.CorePolicyId == request.CorePolicyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Policy), request.CorePolicyId);

        policy.Renew(request.NewCoverageEndDate, request.NewPremiumAmount);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

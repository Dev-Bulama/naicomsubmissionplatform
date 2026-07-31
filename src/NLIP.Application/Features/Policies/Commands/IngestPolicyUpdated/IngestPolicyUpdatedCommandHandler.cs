using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyUpdated;

public class IngestPolicyUpdatedCommandHandler : IRequestHandler<IngestPolicyUpdatedCommand>
{
    private readonly IApplicationDbContext _context;

    public IngestPolicyUpdatedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(IngestPolicyUpdatedCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.Policies.FirstOrDefaultAsync(p => p.CorePolicyId == request.CorePolicyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Policy), request.CorePolicyId);

        policy.ApplyUpdate(request.SumAssured, request.PremiumAmount, request.CoverageEndDate);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyTerminated;

public class IngestPolicyTerminatedCommandHandler : IRequestHandler<IngestPolicyTerminatedCommand>
{
    private readonly IApplicationDbContext _context;

    public IngestPolicyTerminatedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(IngestPolicyTerminatedCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.Policies.FirstOrDefaultAsync(p => p.CorePolicyId == request.CorePolicyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Policy), request.CorePolicyId);

        policy.Terminate(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Policies.Dtos;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Features.Policies.Queries.GetPolicyById;

public record GetPolicyByIdQuery(Guid PolicyId) : IRequest<PolicyDetailDto>;

public class GetPolicyByIdQueryHandler : IRequestHandler<GetPolicyByIdQuery, PolicyDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPolicyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PolicyDetailDto> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Employer)
            .Include(p => p.Branch)
            .Include(p => p.Agent)
            .Include(p => p.Beneficiaries)
            .Include(p => p.GroupMembers)
            .Include(p => p.History)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PolicyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Policy), request.PolicyId);

        var dto = _mapper.Map<PolicyDetailDto>(policy);

        dto.Transactions = await _context.NaicomTransactions
            .Where(t => t.PolicyId == policy.Id)
            .OrderByDescending(t => t.AttemptedAt)
            .ProjectTo<Dtos.NaicomTransactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        dto.SubmissionTimeline = await _context.SubmissionQueue
            .Where(s => s.PolicyId == policy.Id)
            .OrderByDescending(s => s.CreatedAt)
            .ProjectTo<Dtos.SubmissionQueueDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return dto;
    }
}

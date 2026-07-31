using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Policies.Dtos;
using NLIP.Domain.Enums;
using NLIP.Shared.Results;

namespace NLIP.Application.Features.Policies.Queries.GetPolicies;

/// <summary>Backs the Policy Management search screen — every filter listed in the spec
/// (policy number, customer/employer name, NAICOM ID, core policy ID, branch, agent, status,
/// date range, business type) is optional and combinable.</summary>
public record GetPoliciesQuery : IRequest<PaginatedList<PolicySummaryDto>>
{
    public string? PolicyNumber { get; init; }
    public string? CustomerOrEmployerName { get; init; }
    public string? NaicomPolicyId { get; init; }
    public string? CorePolicyId { get; init; }
    public string? BranchCode { get; init; }
    public string? AgentCode { get; init; }
    public PolicyStatus? Status { get; init; }
    public BusinessType? BusinessType { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, PaginatedList<PolicySummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPoliciesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PolicySummaryDto>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Employer)
            .Include(p => p.Branch)
            .Include(p => p.Agent)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.PolicyNumber))
            query = query.Where(p => p.PolicyNumber.Contains(request.PolicyNumber));
        if (!string.IsNullOrWhiteSpace(request.NaicomPolicyId))
            query = query.Where(p => p.NaicomPolicyId != null && p.NaicomPolicyId.Contains(request.NaicomPolicyId));
        if (!string.IsNullOrWhiteSpace(request.CorePolicyId))
            query = query.Where(p => p.CorePolicyId.Contains(request.CorePolicyId));
        if (!string.IsNullOrWhiteSpace(request.BranchCode))
            query = query.Where(p => p.Branch!.Code == request.BranchCode);
        if (!string.IsNullOrWhiteSpace(request.AgentCode))
            query = query.Where(p => p.Agent != null && p.Agent.Code == request.AgentCode);
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status);
        if (request.BusinessType.HasValue)
            query = query.Where(p => p.BusinessType == request.BusinessType);
        if (request.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= request.FromDate.Value);
        if (request.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= request.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(request.CustomerOrEmployerName))
        {
            var term = request.CustomerOrEmployerName;
            query = query.Where(p =>
                (p.Customer != null && (p.Customer.FirstName + " " + p.Customer.LastName).Contains(term)) ||
                (p.Employer != null && p.Employer.Name.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<PolicySummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedList<PolicySummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

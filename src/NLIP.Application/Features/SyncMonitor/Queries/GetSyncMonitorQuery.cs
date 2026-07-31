using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.SyncMonitor.Dtos;
using NLIP.Domain.Enums;
using NLIP.Shared.Results;

namespace NLIP.Application.Features.SyncMonitor.Queries;

public record GetSyncMonitorQuery : IRequest<PaginatedList<SyncMonitorItemDto>>
{
    public SubmissionStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public class GetSyncMonitorQueryHandler : IRequestHandler<GetSyncMonitorQuery, PaginatedList<SyncMonitorItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSyncMonitorQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<SyncMonitorItemDto>> Handle(GetSyncMonitorQuery request, CancellationToken cancellationToken)
    {
        var query =
            from s in _context.SubmissionQueue.AsNoTracking()
            join p in _context.Policies.AsNoTracking() on s.PolicyId equals p.Id
            select new { s, p };

        if (request.Status.HasValue)
            query = query.Where(x => x.s.Status == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new SyncMonitorItemDto
            {
                SubmissionId = x.s.Id,
                PolicyId = x.p.Id,
                PolicyNumber = x.p.PolicyNumber,
                BusinessType = x.p.BusinessType,
                Action = x.s.Action,
                Status = x.s.Status,
                RetryCount = x.s.RetryCount,
                MaxRetries = x.s.MaxRetries,
                LastAttemptAt = x.s.LastAttemptAt,
                NextAttemptAt = x.s.NextAttemptAt,
                LastError = x.s.LastError,
                CreatedAt = x.s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<SyncMonitorItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

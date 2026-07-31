using MediatR;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Common;

namespace NLIP.Application.Common.Behaviors;

/// <summary>
/// Writes one AuditLog row per command (mutating request — anything whose type name ends in
/// "Command"; queries are read-only and intentionally not audited here) before the handler runs.
/// The row is added to the ambient IApplicationDbContext but not saved directly — it rides along
/// with whatever SaveChangesAsync the handler itself performs, so a command that fails validation
/// after this point never leaves an orphaned audit row for an action that didn't happen.
/// </summary>
public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditLoggingBehavior(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Command", StringComparison.Ordinal))
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = _currentUser.UserId,
                UserName = _currentUser.UserName,
                Action = requestName,
                EntityName = requestName.Replace("Command", string.Empty),
                IpAddress = _currentUser.IpAddress
            });
        }

        return await next();
    }
}

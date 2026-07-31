using MediatR;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("NLIP Request: {RequestName} by {UserId} ({UserName})",
            typeof(TRequest).Name, _currentUser.UserId, _currentUser.UserName);
        return await next();
    }
}

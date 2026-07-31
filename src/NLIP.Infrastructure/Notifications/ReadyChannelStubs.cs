using Microsoft.Extensions.Logging;

namespace NLIP.Infrastructure.Notifications;

// "Ready" channels per the spec (SMS/Teams/Slack): the interface and DI wiring exist so a real
// provider (Termii/Twilio for SMS, Teams/Slack incoming webhooks) can be dropped in later by
// implementing these interfaces — nothing else in the codebase needs to change. Until then they
// log instead of sending, so NotificationService never has to special-case "channel not configured".

public interface ISmsSender { Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default); }
public interface ITeamsNotifier { Task SendAsync(string message, CancellationToken cancellationToken = default); }
public interface ISlackNotifier { Task SendAsync(string message, CancellationToken cancellationToken = default); }

public class NoOpSmsSender : ISmsSender
{
    private readonly ILogger<NoOpSmsSender> _logger;
    public NoOpSmsSender(ILogger<NoOpSmsSender> logger) => _logger = logger;
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NLIP SMS (stub — configure Sms:ProviderEndpoint to enable): to {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}

public class NoOpTeamsNotifier : ITeamsNotifier
{
    private readonly ILogger<NoOpTeamsNotifier> _logger;
    public NoOpTeamsNotifier(ILogger<NoOpTeamsNotifier> logger) => _logger = logger;
    public Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NLIP Teams (stub): {Message}", message);
        return Task.CompletedTask;
    }
}

public class NoOpSlackNotifier : ISlackNotifier
{
    private readonly ILogger<NoOpSlackNotifier> _logger;
    public NoOpSlackNotifier(ILogger<NoOpSlackNotifier> logger) => _logger = logger;
    public Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NLIP Slack (stub): {Message}", message);
        return Task.CompletedTask;
    }
}

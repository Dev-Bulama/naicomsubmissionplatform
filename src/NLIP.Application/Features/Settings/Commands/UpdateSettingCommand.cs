using MediatR;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Application.Features.Settings.Commands;

/// <summary>Backs the admin Settings module (API URL, SID/Secret, retry limits, timeouts, queue
/// size, logging level, email/SMS/SignalR/Redis/DB config, encryption keys — see SettingKeys).</summary>
public record UpdateSettingCommand(string Key, string Value, bool IsSecret) : IRequest;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand>
{
    private readonly ISettingsService _settings;

    public UpdateSettingCommandHandler(ISettingsService settings) => _settings = settings;

    public Task Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
        => _settings.SetAsync(request.Key, request.Value, request.IsSecret, cancellationToken);
}

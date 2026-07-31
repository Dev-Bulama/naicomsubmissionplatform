using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Application.Features.Settings.Queries;

public record SettingDto(string Key, string? Value, string? Description, bool IsSecret);

public record GetAllSettingsQuery : IRequest<List<SettingDto>>;

public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, List<SettingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<SettingDto>> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.SystemSettings.AsNoTracking().OrderBy(s => s.Key).ToListAsync(cancellationToken);
        return settings
            // Never return secret values to the client — the settings screen shows a masked
            // placeholder and only overwrites the value when the admin submits a new one.
            .Select(s => new SettingDto(s.Key, s.IsSecret ? "••••••••" : s.Value, s.Description, s.IsSecret))
            .ToList();
    }
}

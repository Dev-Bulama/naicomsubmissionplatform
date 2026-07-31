using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.Settings.Commands;
using NLIP.Application.Features.Settings.Queries;
using NLIP.Shared.Constants;

namespace NLIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionNames.SettingsView)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAllSettingsQuery(), cancellationToken));

    public record UpdateSettingRequest(string Value, bool IsSecret);

    [HttpPut("{key}")]
    [Authorize(Policy = PermissionNames.SettingsManage)]
    public async Task<IActionResult> Update(string key, UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateSettingCommand(key, request.Value, request.IsSecret), cancellationToken);
        return NoContent();
    }
}

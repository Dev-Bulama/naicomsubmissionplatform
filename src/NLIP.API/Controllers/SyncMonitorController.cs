using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.SyncMonitor.Queries;
using NLIP.Shared.Constants;

namespace NLIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionNames.SyncMonitorView)]
public class SyncMonitorController : ControllerBase
{
    private readonly IMediator _mediator;

    public SyncMonitorController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetSyncMonitorQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));
}

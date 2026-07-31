using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.Dashboard.Queries;
using NLIP.Shared.Constants;

namespace NLIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionNames.DashboardView)]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken));
}

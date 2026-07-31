using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.Policies.Commands.IngestPolicyActivated;
using NLIP.Application.Features.Policies.Commands.IngestPolicyRenewed;
using NLIP.Application.Features.Policies.Commands.IngestPolicyTerminated;
using NLIP.Application.Features.Policies.Commands.IngestPolicyUpdated;

namespace NLIP.API.Controllers;

/// <summary>
/// Webhook entry point for the Core Insurance Application connector (see NLIP.Shared docs /
/// ARCHITECTURE.md "Core Application Connectors" — this is the REST/webhook variant; polling and
/// message-queue variants call the same MediatR commands from a different host, e.g. a
/// Hangfire-scheduled poller in NLIP.Worker, without touching this controller). Authenticated
/// with a dedicated IntegrationAdministrator-scoped API key/JWT issued to the Core system, not
/// end-user credentials.
/// </summary>
[ApiController]
[Route("api/core-connector")]
[Authorize(Policy = "Policies.Create")]
public class CoreConnectorController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoreConnectorController(IMediator mediator) => _mediator = mediator;

    [HttpPost("policy-activated")]
    public async Task<IActionResult> PolicyActivated(IngestPolicyActivatedCommand command, CancellationToken cancellationToken)
    {
        var policyId = await _mediator.Send(command, cancellationToken);
        return Accepted(new { policyId });
    }

    [HttpPost("policy-updated")]
    public async Task<IActionResult> PolicyUpdated(IngestPolicyUpdatedCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Accepted();
    }

    [HttpPost("policy-renewed")]
    public async Task<IActionResult> PolicyRenewed(IngestPolicyRenewedCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Accepted();
    }

    [HttpPost("policy-terminated")]
    public async Task<IActionResult> PolicyTerminated(IngestPolicyTerminatedCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Accepted();
    }
}

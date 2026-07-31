using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.Policies.Commands.IngestPolicyRenewed;
using NLIP.Application.Features.Policies.Commands.IngestPolicyTerminated;
using NLIP.Application.Features.Policies.Commands.IngestPolicyUpdated;
using NLIP.Application.Features.Policies.Queries.GetPolicies;
using NLIP.Application.Features.Policies.Queries.GetPolicyById;
using NLIP.Application.Features.Submissions.Commands.RetrySubmission;
using NLIP.Shared.Constants;

namespace NLIP.API.Controllers;

/// <summary>Backs the Policy Management screen: search/filter, detail (with submission
/// timeline/history/logs), and the manual action buttons (Update/Renew/Terminate/Retry).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoliciesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionNames.PoliciesView)]
    public async Task<IActionResult> Search([FromQuery] GetPoliciesQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.PoliciesView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetPolicyByIdQuery(id), cancellationToken));

    [HttpPut("{corePolicyId}/update")]
    [Authorize(Policy = PermissionNames.PoliciesUpdate)]
    public async Task<IActionResult> Update(string corePolicyId, [FromBody] UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new IngestPolicyUpdatedCommand
        {
            CorePolicyId = corePolicyId,
            SumAssured = request.SumAssured,
            PremiumAmount = request.PremiumAmount,
            CoverageEndDate = request.CoverageEndDate
        }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{corePolicyId}/renew")]
    [Authorize(Policy = PermissionNames.PoliciesRenew)]
    public async Task<IActionResult> Renew(string corePolicyId, [FromBody] RenewPolicyRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new IngestPolicyRenewedCommand
        {
            CorePolicyId = corePolicyId,
            NewCoverageEndDate = request.NewCoverageEndDate,
            NewPremiumAmount = request.NewPremiumAmount
        }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{corePolicyId}/terminate")]
    [Authorize(Policy = PermissionNames.PoliciesTerminate)]
    public async Task<IActionResult> Terminate(string corePolicyId, [FromBody] TerminatePolicyRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new IngestPolicyTerminatedCommand { CorePolicyId = corePolicyId, Reason = request.Reason }, cancellationToken);
        return NoContent();
    }

    [HttpPost("submissions/{submissionId:guid}/retry")]
    [Authorize(Policy = PermissionNames.PoliciesRetry)]
    public async Task<IActionResult> RetrySubmission(Guid submissionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RetrySubmissionCommand(submissionId), cancellationToken);
        return NoContent();
    }

    public record UpdatePolicyRequest(decimal SumAssured, decimal PremiumAmount, DateTime CoverageEndDate);
    public record RenewPolicyRequest(DateTime NewCoverageEndDate, decimal? NewPremiumAmount);
    public record TerminatePolicyRequest(string Reason);
}

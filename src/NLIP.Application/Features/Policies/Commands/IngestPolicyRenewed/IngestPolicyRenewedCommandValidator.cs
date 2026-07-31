using FluentValidation;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyRenewed;

public class IngestPolicyRenewedCommandValidator : AbstractValidator<IngestPolicyRenewedCommand>
{
    public IngestPolicyRenewedCommandValidator()
    {
        RuleFor(x => x.CorePolicyId).NotEmpty();
        RuleFor(x => x.NewCoverageEndDate).GreaterThan(DateTime.UtcNow.Date);
    }
}

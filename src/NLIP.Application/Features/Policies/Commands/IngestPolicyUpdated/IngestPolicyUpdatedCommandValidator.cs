using FluentValidation;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyUpdated;

public class IngestPolicyUpdatedCommandValidator : AbstractValidator<IngestPolicyUpdatedCommand>
{
    public IngestPolicyUpdatedCommandValidator()
    {
        RuleFor(x => x.CorePolicyId).NotEmpty();
        RuleFor(x => x.SumAssured).GreaterThan(0);
        RuleFor(x => x.PremiumAmount).GreaterThan(0);
    }
}

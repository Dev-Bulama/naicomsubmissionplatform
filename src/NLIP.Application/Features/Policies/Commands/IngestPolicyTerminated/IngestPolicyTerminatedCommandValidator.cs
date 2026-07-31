using FluentValidation;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyTerminated;

public class IngestPolicyTerminatedCommandValidator : AbstractValidator<IngestPolicyTerminatedCommand>
{
    public IngestPolicyTerminatedCommandValidator()
    {
        RuleFor(x => x.CorePolicyId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

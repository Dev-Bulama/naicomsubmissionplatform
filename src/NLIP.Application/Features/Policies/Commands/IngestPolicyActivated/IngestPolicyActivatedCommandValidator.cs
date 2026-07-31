using FluentValidation;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyActivated;

public class IngestPolicyActivatedCommandValidator : AbstractValidator<IngestPolicyActivatedCommand>
{
    public IngestPolicyActivatedCommandValidator()
    {
        RuleFor(x => x.CorePolicyId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.BranchCode).NotEmpty();
        RuleFor(x => x.SumAssured).GreaterThan(0);
        RuleFor(x => x.PremiumAmount).GreaterThan(0);
        RuleFor(x => x.CoverageEndDate).GreaterThan(x => x.CoverageStartDate);

        When(x => x.BusinessType == BusinessType.IndividualLife, () =>
        {
            RuleFor(x => x.Customer).NotNull().WithMessage("Customer details are required for Individual Life policies.");
            RuleFor(x => x.Customer!.FirstName).NotEmpty().When(x => x.Customer is not null);
            RuleFor(x => x.Customer!.LastName).NotEmpty().When(x => x.Customer is not null);
            RuleFor(x => x.Customer!.PhoneNumber).NotEmpty().When(x => x.Customer is not null);
            RuleForEach(x => x.Beneficiaries).ChildRules(b =>
            {
                b.RuleFor(x => x.FullName).NotEmpty();
                b.RuleFor(x => x.SharePercentage).InclusiveBetween(0, 100);
            });
        });

        When(x => x.BusinessType == BusinessType.GroupLife, () =>
        {
            RuleFor(x => x.Employer).NotNull().WithMessage("Employer details are required for Group Life policies.");
            RuleFor(x => x.Employer!.Name).NotEmpty().When(x => x.Employer is not null);
            RuleFor(x => x.GroupMembers).NotEmpty().WithMessage("At least one group member is required for Group Life policies.");
            RuleForEach(x => x.GroupMembers).ChildRules(m =>
            {
                m.RuleFor(x => x.EmployeeId).NotEmpty();
                m.RuleFor(x => x.FullName).NotEmpty();
                m.RuleFor(x => x.SumAssured).GreaterThan(0);
            });
        });
    }
}

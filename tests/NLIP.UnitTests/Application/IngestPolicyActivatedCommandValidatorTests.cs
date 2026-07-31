using FluentAssertions;
using FluentValidation;
using NLIP.Application.Features.Policies.Commands.IngestPolicyActivated;
using NLIP.Domain.Enums;
using Xunit;

namespace NLIP.UnitTests.Application;

public class IngestPolicyActivatedCommandValidatorTests
{
    private readonly IngestPolicyActivatedCommandValidator _validator = new();

    private static IngestPolicyActivatedCommand BaseCommand(BusinessType type) => new()
    {
        CorePolicyId = "CORE-1",
        PolicyNumber = "POL-1",
        BusinessType = type,
        ProductCode = "IND-LIFE-01",
        BranchCode = "HQ",
        SumAssured = 1_000_000,
        PremiumAmount = 25_000,
        CoverageStartDate = DateTime.UtcNow,
        CoverageEndDate = DateTime.UtcNow.AddYears(1)
    };

    [Fact]
    public void IndividualLife_WithoutCustomer_FailsValidation()
    {
        var command = BaseCommand(BusinessType.IndividualLife);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Customer));
    }

    [Fact]
    public void GroupLife_WithoutMembers_FailsValidation()
    {
        var command = BaseCommand(BusinessType.GroupLife) with
        {
            Employer = new GroupEmployerData("Acme Ltd", "RC12345", null, null, null, null, null)
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.GroupMembers));
    }

    [Fact]
    public void IndividualLife_WithValidCustomer_Passes()
    {
        var command = BaseCommand(BusinessType.IndividualLife) with
        {
            Customer = new IndividualCustomerData("Jane", "Doe", null, DateTime.UtcNow.AddYears(-30), "F", null, null, "08012345678", null, null)
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}

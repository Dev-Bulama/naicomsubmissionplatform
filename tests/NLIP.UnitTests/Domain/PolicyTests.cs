using FluentAssertions;
using NLIP.Domain.Entities.Policies;
using NLIP.Domain.Enums;
using NLIP.Domain.Events;
using NLIP.Domain.Exceptions;
using Xunit;

namespace NLIP.UnitTests.Domain;

public class PolicyTests
{
    private static Policy CreateIndividualDraft() => Policy.CreateDraft(
        policyNumber: "POL-001", corePolicyId: "CORE-001", businessType: BusinessType.IndividualLife,
        productId: Guid.NewGuid(), branchId: Guid.NewGuid(),
        sumAssured: 1_000_000m, premiumAmount: 25_000m, premiumFrequency: "Annual",
        coverageStartDate: DateTime.UtcNow.Date, coverageEndDate: DateTime.UtcNow.Date.AddYears(1),
        agentId: null, customerId: Guid.NewGuid(), employerId: null);

    [Fact]
    public void CreateDraft_IndividualLife_WithoutCustomerId_Throws()
    {
        var act = () => Policy.CreateDraft("POL-002", "CORE-002", BusinessType.IndividualLife,
            Guid.NewGuid(), Guid.NewGuid(), 1000, 100, "Annual", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), null, null, null);

        act.Should().Throw<DomainException>().WithMessage("*CustomerId*");
    }

    [Fact]
    public void CreateDraft_CoverageEndBeforeStart_Throws()
    {
        var act = () => Policy.CreateDraft("POL-003", "CORE-003", BusinessType.IndividualLife,
            Guid.NewGuid(), Guid.NewGuid(), 1000, 100, "Annual", DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), null, Guid.NewGuid(), null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_FromDraft_RaisesPolicyActivatedEvent_AndSetsStatusActive()
    {
        var policy = CreateIndividualDraft();

        policy.Activate();

        policy.Status.Should().Be(PolicyStatus.Active);
        policy.DomainEvents.Should().ContainSingle(e => e is PolicyActivatedEvent);
        policy.History.Should().ContainSingle(h => h.NewStatus == PolicyStatus.Active);
    }

    [Fact]
    public void Activate_WhenNotDraft_Throws()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();

        var act = () => policy.Activate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyUpdate_AfterActivation_RaisesPolicyUpdatedEvent()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();
        policy.ClearDomainEvents();

        policy.ApplyUpdate(1_200_000m, 27_000m, policy.CoverageEndDate);

        policy.SumAssured.Should().Be(1_200_000m);
        policy.DomainEvents.Should().ContainSingle(e => e is PolicyUpdatedEvent);
    }

    [Fact]
    public void Renew_WithEarlierEndDate_Throws()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();

        var act = () => policy.Renew(policy.CoverageEndDate.AddDays(-10));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Renew_WithLaterEndDate_RaisesPolicyRenewedEvent()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();
        policy.ClearDomainEvents();
        var newEnd = policy.CoverageEndDate.AddYears(1);

        policy.Renew(newEnd);

        policy.CoverageEndDate.Should().Be(newEnd);
        policy.Status.Should().Be(PolicyStatus.Renewed);
        policy.DomainEvents.Should().ContainSingle(e => e is PolicyRenewedEvent);
    }

    [Fact]
    public void Terminate_RaisesPolicyCancelledEvent_AndIsIdempotentlyBlocked()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();
        policy.ClearDomainEvents();

        policy.Terminate("Non-payment of premium");

        policy.Status.Should().Be(PolicyStatus.Terminated);
        policy.DomainEvents.Should().ContainSingle(e => e is PolicyCancelledEvent);

        var act = () => policy.Terminate("Second attempt");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddGroupMember_OnIndividualLifePolicy_Throws()
    {
        var policy = CreateIndividualDraft();

        var act = () => policy.AddGroupMember("EMP1", "Jane Doe", DateTime.UtcNow.AddYears(-30), "F", 500_000m, "Analyst", DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkSubmissionResult_OnSuccess_ResetsRetryCount()
    {
        var policy = CreateIndividualDraft();
        policy.Activate();

        policy.MarkSubmissionResult(SubmissionStatus.Retrying, incrementRetry: true);
        policy.MarkSubmissionResult(SubmissionStatus.Retrying, incrementRetry: true);
        policy.RetryCount.Should().Be(2);

        policy.MarkSubmissionResult(SubmissionStatus.Completed, incrementRetry: false);
        policy.RetryCount.Should().Be(0);
    }
}

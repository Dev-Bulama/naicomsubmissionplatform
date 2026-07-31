using MediatR;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyActivated;

/// <summary>
/// Entry point for the Core Application Connector (webhook, polling job, or message-queue
/// consumer — see NLIP.Infrastructure.CoreConnectors) when a policy is activated in the Core
/// Insurance Application. Creates the local Policy record if unseen and raises PolicyActivatedEvent,
/// which the outbox pipeline turns into a NAICOM Create-Policy submission.
/// </summary>
public record IngestPolicyActivatedCommand : IRequest<Guid>
{
    public required string CorePolicyId { get; init; }
    public required string PolicyNumber { get; init; }
    public required BusinessType BusinessType { get; init; }
    public required string ProductCode { get; init; }
    public required string BranchCode { get; init; }
    public string? AgentCode { get; init; }

    public required decimal SumAssured { get; init; }
    public required decimal PremiumAmount { get; init; }
    public string PremiumFrequency { get; init; } = "Annual";
    public required DateTime CoverageStartDate { get; init; }
    public required DateTime CoverageEndDate { get; init; }

    // Individual Life
    public IndividualCustomerData? Customer { get; init; }
    public List<BeneficiaryData> Beneficiaries { get; init; } = new();

    // Group Life
    public GroupEmployerData? Employer { get; init; }
    public List<GroupMemberData> GroupMembers { get; init; } = new();
}

public record IndividualCustomerData(
    string FirstName, string LastName, string? MiddleName, DateTime DateOfBirth,
    string Gender, string? NationalIdNumber, string? Bvn, string PhoneNumber, string? Email, string? Address);

public record BeneficiaryData(string FullName, string Relationship, DateTime DateOfBirth, decimal SharePercentage, string? PhoneNumber, string? Email);

public record GroupEmployerData(string Name, string? RcNumber, string? Address, string? ContactPerson, string? ContactEmail, string? ContactPhone, string? TaxIdentificationNumber);

public record GroupMemberData(string EmployeeId, string FullName, DateTime DateOfBirth, string Gender, decimal SumAssured, string? Designation, DateTime DateJoined);

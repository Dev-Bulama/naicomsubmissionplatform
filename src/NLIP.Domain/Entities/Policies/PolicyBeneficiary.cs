using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Policies;

public class PolicyBeneficiary : BaseAuditableEntity
{
    public Guid PolicyId { get; private set; }
    public Policy? Policy { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Relationship { get; private set; } = default!;
    public DateTime DateOfBirth { get; private set; }
    public decimal SharePercentage { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }

    private PolicyBeneficiary() { }

    internal PolicyBeneficiary(Guid policyId, string fullName, string relationship, DateTime dateOfBirth, decimal sharePercentage, string? phoneNumber, string? email)
    {
        PolicyId = policyId;
        FullName = fullName;
        Relationship = relationship;
        DateOfBirth = dateOfBirth;
        SharePercentage = sharePercentage;
        PhoneNumber = phoneNumber;
        Email = email;
    }
}

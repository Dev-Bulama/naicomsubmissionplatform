using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Policies;

/// <summary>A covered employee under a Group Life scheme policy.</summary>
public class GroupMember : BaseAuditableEntity
{
    public Guid PolicyId { get; private set; }
    public Policy? Policy { get; private set; }
    public string EmployeeId { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public DateTime DateOfBirth { get; private set; }
    public string Gender { get; private set; } = default!;
    public decimal SumAssured { get; private set; }
    public string? Designation { get; private set; }
    public DateTime DateJoined { get; private set; }
    public bool IsActive { get; private set; } = true;

    private GroupMember() { }

    internal GroupMember(Guid policyId, string employeeId, string fullName, DateTime dateOfBirth, string gender, decimal sumAssured, string? designation, DateTime dateJoined)
    {
        PolicyId = policyId;
        EmployeeId = employeeId;
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        SumAssured = sumAssured;
        Designation = designation;
        DateJoined = dateJoined;
    }

    public void Deactivate() => IsActive = false;
}

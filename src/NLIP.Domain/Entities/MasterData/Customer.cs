using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.MasterData;

/// <summary>The insured principal for an Individual Life policy.</summary>
public class Customer : BaseAuditableEntity
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default!;
    public string? NationalIdNumber { get; set; }
    public string? Bvn { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));
}

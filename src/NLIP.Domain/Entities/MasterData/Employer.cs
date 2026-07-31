using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.MasterData;

/// <summary>The employer/master policyholder for a Group Life scheme.</summary>
public class Employer : BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? RcNumber { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

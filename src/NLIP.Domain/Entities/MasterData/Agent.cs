using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.MasterData;

public class Agent : BaseAuditableEntity
{
    public string Code { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

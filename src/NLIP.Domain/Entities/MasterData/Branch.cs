using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.MasterData;

public class Branch : BaseAuditableEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? State { get; set; }
    public bool IsActive { get; set; } = true;
}

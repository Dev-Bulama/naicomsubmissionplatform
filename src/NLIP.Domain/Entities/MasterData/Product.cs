using NLIP.Domain.Common;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Entities.MasterData;

public class Product : BaseAuditableEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public BusinessType BusinessType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

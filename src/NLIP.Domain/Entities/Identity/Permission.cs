using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

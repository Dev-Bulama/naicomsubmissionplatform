using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Identity;

public class Role : BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();
}

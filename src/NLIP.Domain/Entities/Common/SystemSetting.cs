using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Common;

/// <summary>Key/value configuration row surfaced through the admin Settings module (see
/// NLIP.Shared.Constants.SettingKeys for the well-known keys). Secret values (IsSecret) are
/// encrypted at rest by NLIP.Infrastructure.Security.SettingEncryptionService before being stored.</summary>
public class SystemSetting : BaseAuditableEntity
{
    public string Key { get; set; } = default!;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
}

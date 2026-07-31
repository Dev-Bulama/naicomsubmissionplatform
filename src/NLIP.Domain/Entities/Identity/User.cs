using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Identity;

public class User : BaseAuditableEntity
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;

    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    /// <summary>MFA-ready: TOTP secret populated once the user enables 2FA. Not enforced in this
    /// release's login flow, but the column and services are in place for a future rollout.</summary>
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
}

using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Token { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}

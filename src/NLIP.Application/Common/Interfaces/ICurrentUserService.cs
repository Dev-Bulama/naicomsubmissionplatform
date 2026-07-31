namespace NLIP.Application.Common.Interfaces;

/// <summary>Resolves the acting user from the current HTTP/SignalR context. Implemented in the
/// host layer (API/Web) since only they have access to HttpContext; Application only ever reads it.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? IpAddress { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}

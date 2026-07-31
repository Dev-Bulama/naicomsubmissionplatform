using Microsoft.AspNetCore.SignalR;

namespace NLIP.Infrastructure.Realtime;

/// <summary>Maps the JWT "sub" claim to SignalR's Context.UserIdentifier (the default provider
/// looks for ClaimTypes.NameIdentifier, which this platform's tokens don't set) so
/// NotificationHub can group connections per user.</summary>
public class JwtUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
}

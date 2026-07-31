using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace NLIP.Web.Services;

public class NlipAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthSessionService _session;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public NlipAuthStateProvider(AuthSessionService session)
    {
        _session = session;
        _session.Changed += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var auth = _session.Current ?? await _session.RestoreAsync();
        if (auth is null) return new AuthenticationState(Anonymous);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.UserName),
            new("full_name", auth.FullName)
        };
        claims.AddRange(auth.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(auth.Permissions.Select(p => new Claim("permission", p)));

        var identity = new ClaimsIdentity(claims, "NLIP");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}

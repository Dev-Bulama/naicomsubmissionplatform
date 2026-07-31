using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using NLIP.Shared.Constants;

namespace NLIP.Web.Services;

/// <summary>Mirrors NLIP.API.Security.PermissionPolicyProvider so &lt;AuthorizeView Policy="..."&gt;
/// in Razor components can reference any Shared.Constants.PermissionNames value directly — the
/// nav menu and page-level [Authorize] attributes both check the "permission" claim populated
/// into NlipAuthStateProvider from the login response.</summary>
public class WebPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public WebPermissionPolicyProvider(IOptions<AuthorizationOptions> options) => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (PermissionNames.All.Contains(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ClaimsAuthorizationRequirement("permission", new[] { policyName }))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return _fallback.GetPolicyAsync(policyName);
    }
}

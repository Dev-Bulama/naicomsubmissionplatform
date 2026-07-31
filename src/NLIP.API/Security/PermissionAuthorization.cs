using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NLIP.Shared.Constants;

namespace NLIP.API.Security;

/// <summary>
/// Lets controllers write [Authorize(Policy = PermissionNames.PoliciesCreate)] for any of the
/// ~16 permissions in Shared.Constants.PermissionNames without pre-registering one
/// AddPolicy(...) call per permission: the policy name IS the permission, and this provider
/// synthesizes a ClaimsAuthorizationRequirement("permission", policyName) on first use. Actual
/// role -> permission assignment is configurable at runtime (RolePermissions table) — this class
/// only checks "does the caller's token carry this permission claim", which LoginCommandHandler
/// populates from that table at login time.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

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

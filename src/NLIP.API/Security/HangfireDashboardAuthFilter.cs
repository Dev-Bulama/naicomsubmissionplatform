using Hangfire.Dashboard;

namespace NLIP.API.Security;

/// <summary>Restricts /hangfire (Background Jobs dashboard — queue monitoring, retry schedule,
/// dead letters) to authenticated SuperAdministrator/SystemAdministrator/IntegrationAdministrator
/// users; everyone else gets a 401/redirect to login.</summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext.User.Identity?.IsAuthenticated != true) return false;

        return httpContext.User.IsInRole(Shared.Constants.RoleNames.SuperAdministrator)
            || httpContext.User.IsInRole(Shared.Constants.RoleNames.SystemAdministrator)
            || httpContext.User.IsInRole(Shared.Constants.RoleNames.IntegrationAdministrator);
    }
}

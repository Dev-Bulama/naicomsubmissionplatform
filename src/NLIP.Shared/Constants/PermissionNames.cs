namespace NLIP.Shared.Constants;

/// <summary>
/// Granular permission strings checked via policy-based authorization (AuthorizeAttribute(Policy = ...)).
/// Stored in the Permissions table and assigned to roles via RolePermissions so admins can reconfigure
/// access without a code change.
/// </summary>
public static class PermissionNames
{
    public const string PoliciesView = "Policies.View";
    public const string PoliciesCreate = "Policies.Create";
    public const string PoliciesUpdate = "Policies.Update";
    public const string PoliciesRenew = "Policies.Renew";
    public const string PoliciesTerminate = "Policies.Terminate";
    public const string PoliciesDelete = "Policies.Delete";
    public const string PoliciesRetry = "Policies.Retry";

    public const string DashboardView = "Dashboard.View";
    public const string SyncMonitorView = "SyncMonitor.View";
    public const string AuditTrailView = "AuditTrail.View";
    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";

    public const string SettingsView = "Settings.View";
    public const string SettingsManage = "Settings.Manage";

    public const string UsersManage = "Users.Manage";
    public const string RolesManage = "Roles.Manage";

    public static readonly IReadOnlyList<string> All = new[]
    {
        PoliciesView, PoliciesCreate, PoliciesUpdate, PoliciesRenew, PoliciesTerminate, PoliciesDelete, PoliciesRetry,
        DashboardView, SyncMonitorView, AuditTrailView, ReportsView, ReportsExport,
        SettingsView, SettingsManage, UsersManage, RolesManage
    };
}

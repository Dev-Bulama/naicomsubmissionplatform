namespace NLIP.Shared.Constants;

/// <summary>Fixed system role names. Permissions assigned per-role are configurable at runtime (RolePermission table).</summary>
public static class RoleNames
{
    public const string SuperAdministrator = "SuperAdministrator";
    public const string SystemAdministrator = "SystemAdministrator";
    public const string IntegrationAdministrator = "IntegrationAdministrator";
    public const string ComplianceOfficer = "ComplianceOfficer";
    public const string OperationsOfficer = "OperationsOfficer";
    public const string Auditor = "Auditor";
    public const string ReadOnlyUser = "ReadOnlyUser";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdministrator, SystemAdministrator, IntegrationAdministrator,
        ComplianceOfficer, OperationsOfficer, Auditor, ReadOnlyUser
    };
}

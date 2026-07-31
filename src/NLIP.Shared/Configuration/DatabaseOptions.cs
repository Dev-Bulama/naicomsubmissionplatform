namespace NLIP.Shared.Configuration;

public enum DatabaseProvider
{
    SqlServer = 0,
    Postgres = 1
}

/// <summary>
/// Bound from the "Database" config section. Lets NLIP.Persistence (EF Core provider) and
/// NLIP.Infrastructure (Hangfire storage) agree on which database engine is active without
/// either one hardcoding the other's concern. Defaults to SqlServer (the tech-stack requirement
/// this platform was originally built against); set Database:Provider=Postgres for the Render
/// deployment path, which uses Render's managed Postgres instead of a self-hosted SQL Server —
/// see docs/DEPLOYMENT.md and render.yaml.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
}

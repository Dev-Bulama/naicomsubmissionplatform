namespace NLIP.Shared.Configuration;

/// <summary>
/// Render (and most other PaaS providers — Heroku, Railway) hand out managed Postgres
/// connection info as a single libpq/JDBC-style URI (`postgres://user:pass@host:port/db`), not
/// the semicolon key=value format Npgsql's connection string parser actually requires
/// (`Host=...;Database=...;Username=...;Password=...`) — passing the URI straight to
/// NpgsqlConnection throws "Format of the initialization string does not conform to
/// specification." This converts one into the other; used by both NLIP.Persistence (EF Core) and
/// NLIP.Infrastructure (Hangfire.PostgreSql) so the same render.yaml-supplied
/// ConnectionStrings__DefaultConnection value works for both. A no-op for SQL Server connection
/// strings or already-Npgsql-formatted strings.
/// </summary>
public static class ConnectionStringNormalizer
{
    public static string NormalizePostgresUri(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require";
    }
}

# Deployment Guide

## 1. Docker (recommended)

```bash
cp .env.example .env    # set DB_PASSWORD, JWT_SIGNING_KEY, ENCRYPTION_KEY_BASE64
docker compose up --build -d
```

Services: `sqlserver`, `redis`, `api` (port 8080), `worker` (no exposed port — Hangfire server
only), `web` (port 8081). Volumes persist SQL Server data, Redis data, and API/Worker log files
across restarts.

Generate the secrets before first run:

```bash
openssl rand -base64 48   # JWT_SIGNING_KEY
openssl rand -base64 32   # ENCRYPTION_KEY_BASE64
```

**Before the API can serve traffic**, an EF Core migration must exist (see
`docs/ROADMAP.md#generate-the-initial-ef-core-migration`) — this scaffold's `Database.MigrateAsync()`
call is a no-op with zero migration files present.

## 2. IIS deployment (Windows Server)

1. Install the .NET 9 Hosting Bundle on the IIS server.
2. Publish each web-facing project as a self-contained or framework-dependent deployment:
   ```bash
   dotnet publish src/NLIP.API -c Release -o C:\inetpub\nlip-api
   dotnet publish src/NLIP.Web -c Release -o C:\inetpub\nlip-web
   ```
3. Create two IIS sites (or one site + one app under it), each pointing at its publish folder,
   both running the ASP.NET Core Module (in-process hosting).
4. Set environment variables on each site's Application Pool (or via `web.config`
   `<environmentVariables>`): `ConnectionStrings__DefaultConnection`, `ConnectionStrings__Redis`,
   `Jwt__SigningKey`, `Encryption__Key`, `Naicom__BaseUrl`. Never bake secrets into `web.config`
   in source control — set them on the server (IIS Manager -> Configuration Editor, or via
   `appcmd`), or better, via Azure Key Vault (see below) if the box has managed identity access.
5. Run `NLIP.Worker` as a Windows Service (it has no HTTP listener):
   ```bash
   dotnet publish src/NLIP.Worker -c Release -o C:\Services\nlip-worker
   sc create NlipWorker binPath= "C:\Services\nlip-worker\NLIP.Worker.exe"
   sc start NlipWorker
   ```
6. Point IIS's site bindings at your TLS certificate; do not terminate TLS anywhere except at IIS
   or an upstream load balancer you control.

## 3. Azure deployment

- **App Service** (Linux, .NET 9) for `NLIP.API` and `NLIP.Web` — one App Service per project, or
  one Service Plan hosting both as separate Web Apps.
- **Azure Container Apps** or a small **VM Scale Set** for `NLIP.Worker` (it's a long-running
  background process, not a good fit for App Service's request-driven model unless you use Azure
  WebJobs — Container Apps with a single always-on replica is simpler).
- **Azure SQL Database** for `DefaultConnection`.
- **Azure Cache for Redis** for `ConnectionStrings:Redis`.
- **Azure Key Vault** for `Jwt:SigningKey`, `Encryption:Key`, `Naicom:Secret` — every secret in
  this app is read through `IConfiguration`, so wiring
  `builder.Configuration.AddAzureKeyVault(...)` with a managed identity is a drop-in addition to
  each host's `Program.cs`, no other code changes needed ("Key Vault Ready" per the security spec).
- **Application Insights** for the request/dependency telemetry Serilog doesn't cover natively —
  add `Serilog.Sinks.ApplicationInsights` alongside the existing sinks in
  `NLIP.Infrastructure.Logging.SerilogConfigurator`.

## 4. CI/CD

`.github/workflows/ci.yml` builds the solution and runs both test projects on every push/PR. Wire
a deploy job onto it once you have a target environment: for Azure, `azure/webapps-deploy@v3`
after `dotnet publish`; for on-prem, an IIS/PowerShell remoting step or a self-hosted runner with
`dotnet publish` + `robocopy`/`web deploy`. Azure DevOps: the same three commands
(`restore` / `build` / `test`) map directly onto a `dotnet` task-based YAML pipeline.

## 5. Database migrations in production

Prefer generating a SQL script and reviewing it rather than letting `Database.MigrateAsync()` run
untested DDL against production:

```bash
dotnet ef migrations script --idempotent -p src/NLIP.Persistence -s src/NLIP.API -o migrate.sql
```

Review `migrate.sql`, then run it through your normal DBA change-control process. Reserve
`Database.MigrateAsync()` (as wired in `NLIP.API/Program.cs`) for dev/staging convenience.

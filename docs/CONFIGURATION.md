# Configuration Guide

Two configuration layers exist, deliberately:

1. **Boot-time config** (`appsettings.json` + environment variables) — connection strings, JWT
   issuer/audience, initial NAICOM base URL. Read once at startup via `IConfiguration`/`IOptions<T>`.
2. **Runtime-editable config** (`SystemSettings` table, via `ISettingsService`) — everything an
   administrator should be able to change from the Settings screen without a redeploy: NAICOM
   SID/Secret, retry limits, timeouts, logging level, email/SMS settings, session/lockout policy.
   Cached for 5 minutes (`ICacheService`) and invalidated immediately on write.

## Well-known runtime settings (`NLIP.Shared.Constants.SettingKeys`)

| Key | Default (seeded) | Purpose |
|---|---|---|
| `Naicom:BaseUrl` | `https://portal.naicom.gov.ng/` | NAICOM Portal base URL |
| `Naicom:Sid` / `Naicom:Secret` | empty — **set before first submission** | NAICOM auth credentials, encrypted at rest |
| `Naicom:TimeoutSeconds` | 30 | Per-attempt HTTP timeout |
| `Retry:MaxAttempts` | 6 | Submissions dead-letter after this many failed attempts |
| `Retry:ScheduleCsv` | `30,60,300,900,1800,3600` | Documented backoff schedule (seconds) — the enforced source of truth is `SubmissionQueue.RetryScheduleSeconds` in code; keep this setting in sync if you change one |
| `Queue:MaxConcurrency` | 10 | NAICOM client bulkhead (`NaicomOptions.BulkheadMaxParallelization`) |
| `Logging:MinimumLevel` | Information | Serilog minimum level |
| `Email:SmtpHost/Port/FromAddress` | empty | SMTP relay for email notifications; unset = emails are skipped (logged, not sent) |
| `Sms:ProviderEndpoint` | empty | Placeholder for a future SMS provider integration |
| `Security:SessionTimeoutMinutes` | 30 | Idle session timeout |
| `Security:AccountLockoutThreshold/Minutes` | 5 / 15 | Failed-login lockout policy |

Update via `PUT /api/settings/{key}` (requires `Settings.Manage` permission) or the Settings page
in the Web UI.

## Boot-time configuration keys

| Section | Key | Notes |
|---|---|---|
| `Database` | `Provider` | `SqlServer` (default, Docker Compose/on-prem) or `Postgres` (Render — see `docs/DEPLOYMENT.md`). Selects both the EF Core provider and which of the two migration sets (`Migrations/SqlServer`, `Migrations/Postgres`) applies, plus Hangfire's storage engine |
| `ConnectionStrings` | `DefaultConnection` | SQL Server or Postgres connection string depending on `Database:Provider`. A `postgres://` URI (as Render supplies) is auto-converted to Npgsql format — see `ConnectionStringNormalizer`. `Redis` — optional; empty falls back to in-memory distributed cache |
| `Jwt` | `Issuer`, `Audience`, `SigningKey`, `AccessTokenMinutes` | `SigningKey` must be a strong random value — see `.env.example` |
| `Encryption` | `Key` | Base64 32-byte AES key for encrypting secret settings |
| `Naicom` | `BaseUrl`, `TimeoutSeconds`, `RetryCount`, `CircuitBreakerFailureThreshold`, `CircuitBreakerBreakSeconds`, `BulkheadMaxParallelization`, `BulkheadMaxQueuedActions` | Resilience pipeline tuning — see `NaicomResiliencePolicies` |
| `Cors` | `AllowedOrigins` | Must include the Web app's origin |
| `IpRateLimiting` | AspNetCoreRateLimit schema | Per-endpoint/global request caps |
| `Api` (Web project only) | `BaseUrl` | Where NLIP.Web finds NLIP.API |

All of the above can be set via environment variables using the standard ASP.NET Core
double-underscore convention (e.g. `Jwt__SigningKey`, `ConnectionStrings__DefaultConnection`), which
is how `docker-compose.yml` supplies them.

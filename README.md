# NAICOM Life Assurance Integration Platform (NLIP)

A middleware integration platform that synchronizes **Individual Life (Retail)** and **Group
Life** policies from a Core Insurance Application to the NAICOM Portal API. Every policy
activation, update, renewal, and termination in the Core system is automatically detected,
mapped, submitted to NAICOM, retried on failure, and fully audited — with no other insurance
class in scope.

> **Read this first:** [`docs/ROADMAP.md`](docs/ROADMAP.md) lists exactly what in this codebase is
> production-real vs. a scaffold/stub. The solution builds cleanly and all 20 tests pass under a
> real .NET SDK (see the roadmap for what that pass found and fixed) and ships EF Core migrations
> for both supported database providers. The one still-unverified piece: the NAICOM DTOs are a
> best-effort design, not a transcription of the live spec — this environment could not reach
> `portal.naicom.gov.ng` to confirm field-level accuracy. Read the roadmap before assuming any
> given piece is finished.

## Solution layout

```
/src
  NLIP.Domain          Entities, enums, domain events, business rules (no external dependencies)
  NLIP.Application     CQRS (MediatR) use cases, validation, DTOs, port interfaces
  NLIP.Integration     NAICOM API client, DTOs, Polly resilience, AutoMapper profiles
  NLIP.Persistence     EF Core DbContext, entity configurations, repositories, outbox interceptor
  NLIP.Infrastructure  Serilog, JWT/security, caching, notifications, Hangfire wiring
  NLIP.Worker          Hangfire server: outbox processor, retry engine, health/token jobs
  NLIP.API             ASP.NET Core Web API: JWT auth, RBAC, Swagger, SignalR, rate limiting
  NLIP.Web             Blazor Server dashboard UI
  NLIP.Shared          Cross-cutting constants/results with no other dependencies
/tests
  NLIP.UnitTests           Domain rules, validators, security
  NLIP.IntegrationTests    WebApplicationFactory + Sqlite in-memory API tests
/deploy
  docker/              Dockerfiles for API/Web/Worker
  sql/                 schema.sql / schema.postgres.sql — generated from the real EF Core migrations
/docs                  Architecture, deployment, security, configuration, ops, roadmap, manuals
render.yaml            Render Blueprint (Postgres deployment path — see docs/DEPLOYMENT.md)
```

## Architecture

Clean Architecture / DDD / CQRS, one direction of dependency only:

```
Presentation (API, Web) -> Application -> Domain
Infrastructure, Persistence, Integration -> Application (implement its interfaces)
```

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full diagram set (component diagram,
submission sequence diagram, ER diagram) and the design-pattern rationale (Outbox, Repository +
Unit of Work, CQRS, resilience pipeline, etc).

## Quick start (Docker, SQL Server)

```bash
cp .env.example .env   # fill in DB_PASSWORD, JWT_SIGNING_KEY, ENCRYPTION_KEY_BASE64
docker compose up --build
```

- API + Swagger: http://localhost:8080/swagger
- Web dashboard: http://localhost:8081
- Hangfire dashboard: http://localhost:8080/hangfire (SuperAdministrator/SystemAdministrator/IntegrationAdministrator only)
- Default seeded login: `admin` / `ChangeMe!2026` (forced password change on first login — see `MustChangePassword`)

The EF Core migration under `src/NLIP.Persistence/Migrations/SqlServer` applies automatically on
first boot via `Database.MigrateAsync()`.

## Quick start (Render, Postgres)

Push this repo to GitHub/GitLab, then in the Render dashboard: **New +** -> **Blueprint** -> select
the repo. `render.yaml` provisions managed Postgres, managed Redis, and all three app services in
one pass. See [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md#2-render-one-click-blueprint-postgres) for
the post-deploy steps (setting NAICOM credentials) and a caveat about verifying the Blueprint
schema against Render's current docs.

## Local development (without Docker)

Requires the .NET 9 SDK (or a later major with `RollForward` — see `global.json` /
`Directory.Build.props`) and a reachable SQL Server or Postgres instance, plus Redis (optional —
falls back to in-memory distributed cache if `ConnectionStrings:Redis` is empty).

```bash
dotnet restore
dotnet ef database update -p src/NLIP.Persistence -s src/NLIP.API   # applies Migrations/SqlServer
dotnet run --project src/NLIP.API
dotnet run --project src/NLIP.Worker
dotnet run --project src/NLIP.Web
```

To run against Postgres locally instead, set `Database:Provider=Postgres` and point
`ConnectionStrings:DefaultConnection` at it — `Migrations/Postgres` applies instead (see
`ProviderFilteredMigrationsAssembly`).

## Scope

Only **Individual Life** and **Group Life** NAICOM endpoints are implemented, per the task brief.
Authentication/health/version endpoints are shared infrastructure. Every other NAICOM insurance
class is intentionally out of scope; the architecture (BusinessType enum, DTO mapping layer,
NaicomApiEndpoints constants) is built so a future class can be added without touching the outbox,
retry, audit, or dashboard code.

## Documentation index

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — diagrams, patterns, data flow
- [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) — Docker, IIS, Azure, CI/CD
- [`docs/SECURITY.md`](docs/SECURITY.md) — security checklist
- [`docs/CONFIGURATION.md`](docs/CONFIGURATION.md) — every setting, where it lives, how to change it
- [`docs/MONITORING.md`](docs/MONITORING.md) — health checks, logs, Hangfire dashboard, alerts
- [`docs/DISASTER_RECOVERY.md`](docs/DISASTER_RECOVERY.md) — backup/restore, failover
- [`docs/ROADMAP.md`](docs/ROADMAP.md) — what's real vs. stubbed, and what's next
- [`docs/USER_MANUAL.md`](docs/USER_MANUAL.md) — for Operations/Compliance/Auditor users
- [`docs/ADMIN_MANUAL.md`](docs/ADMIN_MANUAL.md) — for System/Integration/Super Administrators

# NAICOM Life Assurance Integration Platform (NLIP)

A middleware integration platform that synchronizes **Individual Life (Retail)** and **Group
Life** policies from a Core Insurance Application to the NAICOM Portal API. Every policy
activation, update, renewal, and termination in the Core system is automatically detected,
mapped, submitted to NAICOM, retried on failure, and fully audited — with no other insurance
class in scope.

> **Read this first:** [`docs/ROADMAP.md`](docs/ROADMAP.md) lists exactly what in this codebase is
> production-real vs. a scaffold/stub, and the concrete steps left before go-live (most
> importantly: generating the first EF Core migration, and verifying the NAICOM DTOs against the
> live API spec — this environment could not reach `portal.naicom.gov.ng` to confirm field-level
> accuracy). Read it before assuming any given piece is finished.

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
  sql/                 Hand-authored reference schema (see docs/ROADMAP.md)
/docs                  Architecture, deployment, security, configuration, ops, roadmap, manuals
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

## Quick start (Docker)

```bash
cp .env.example .env   # fill in DB_PASSWORD, JWT_SIGNING_KEY, ENCRYPTION_KEY_BASE64
docker compose up --build
```

- API + Swagger: http://localhost:8080/swagger
- Web dashboard: http://localhost:8081
- Hangfire dashboard: http://localhost:8080/hangfire (SuperAdministrator/SystemAdministrator/IntegrationAdministrator only)
- Default seeded login: `admin` / `ChangeMe!2026` (forced password change on first login — see `MustChangePassword`)

Before this actually creates tables, generate the EF Core migration (see ROADMAP) — this scaffold
does not ship one because the sandbox that built it had no .NET SDK available.

## Local development (without Docker)

Requires the .NET 9 SDK, a reachable SQL Server, and Redis (optional — falls back to in-memory
distributed cache if `ConnectionStrings:Redis` is empty).

```bash
dotnet restore
dotnet ef migrations add InitialCreate -p src/NLIP.Persistence -s src/NLIP.API
dotnet run --project src/NLIP.API
dotnet run --project src/NLIP.Worker
dotnet run --project src/NLIP.Web
```

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

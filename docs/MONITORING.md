# Monitoring Guide

## Health checks

`GET /health` on `NLIP.API` runs `AddDbContextCheck<NlipDbContext>` — wire it into your load
balancer / uptime monitor. Extend with `AddCheck` calls for Redis and NAICOM reachability
(`INaicomApiClient.CheckHealthAsync` already exists and is exercised every 2 minutes by
`NaicomHealthCheckJob` in `NLIP.Worker` — a `IHealthCheck` wrapper around it is a small addition).

## Background jobs

`GET /hangfire` (role-restricted, see `SECURITY.md`) shows: the live queue, retry schedule per
job, failed/succeeded counts, and the three recurring jobs:

- `outbox-processor` — every 15s, drains due `SubmissionQueue` rows
- `naicom-health-check` — every 2 minutes, API Health Monitor probe
- `naicom-token-warmup` — every 45 minutes, proactive token refresh

## Logs (Serilog)

Every host (`API`, `Web`, `Worker`) writes to the same three sinks via
`NLIP.Infrastructure.Logging.SerilogConfigurator`:

- **Console** — container stdout, scraped by your platform's log collector (Docker/Kubernetes/App Service)
- **Rolling file** — `logs/nlip-.log`, daily rotation, 30-day retention
- **SQL Server** — `SerilogLogs` table (auto-created), queryable for ad-hoc investigation

Every log line carries a `CorrelationId` (from `CorrelationIdMiddleware`, or the `X-Correlation-Id`
header the Core Application connector sends) — grep/query on that to trace one submission across
API -> outbox -> Worker -> NAICOM. Add `Serilog.Sinks.Elasticsearch` alongside the existing
`WriteTo` chain to ship to ElasticSearch without touching call sites elsewhere.

## Dashboards to build first

1. **Executive Dashboard** (`GET /api/dashboard/stats`, already in the Web UI) — success/failure
   rate, retry queue depth, NAICOM availability.
2. **Synchronization Monitor** (`GET /api/syncmonitor`) — live per-submission status; use this,
   not raw Hangfire, for policy-level troubleshooting.
3. Alert on: `SubmissionQueue` rows reaching `DeadLetter`, `NaicomHealthCheckJob` reporting
   `IsAvailable = false` for more than one consecutive check, and Hangfire's own recommended
   alerts (server heartbeat loss, queue length growth).

## What isn't wired up yet

ElasticSearch sink, Application Insights/OpenTelemetry tracing, and a dedicated alerting
integration (PagerDuty/Opsgenie) are not included — see `docs/ROADMAP.md`.

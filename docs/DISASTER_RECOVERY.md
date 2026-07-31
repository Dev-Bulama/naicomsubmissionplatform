# Disaster Recovery Guide

## What must survive a disaster

| Data | Where | RPO target | Recovery approach |
|---|---|---|---|
| Policies, submissions, transactions, audit logs | SQL Server (`NlipDb`) | Minutes | Point-in-time restore (Azure SQL) or `sqlcmd`/native backup restore for self-hosted SQL Server; the `SubmissionQueue`/Outbox design means any submission not yet acknowledged by NAICOM simply re-processes once the restored DB and Worker come back up — no data is silently lost mid-flight because the outbox row and the triggering entity change are one transaction |
| Redis (cache) | Redis | Seconds — but disposable | Cache-only; safe to lose entirely, `ICacheService.GetOrCreateAsync` repopulates on next read. Do not treat Redis as a source of truth for anything |
| Hangfire job state | SQL Server (same DB) | Minutes | Recovered with the SQL Server restore; in-flight jobs at the moment of failure are re-picked-up by Hangfire's own recovery (jobs held past their invisibility timeout are requeued automatically) |
| Application secrets | Key Vault / environment | N/A | Re-provision from Key Vault (not from the failed instance) |

## Backup strategy

- **SQL Server:** full backup daily, differential every 6 hours, transaction log backup every 15
  minutes (or use Azure SQL's automated point-in-time restore, default 7-35 day retention).
- **Redis:** no backup needed (see above) — if using Azure Cache for Redis, disable persistence to
  avoid paying for a guarantee this app doesn't need.
- **Application code/config:** source control is the backup; secrets live in Key Vault which has
  its own soft-delete/purge-protection.

## Failover

- Run `NLIP.API` and `NLIP.Web` behind a load balancer with 2+ instances each — both are stateless
  aside from the Blazor Server circuit (a lost `NLIP.Web` instance drops connected users' live
  circuits; they reconnect and `AuthSessionService`'s `ProtectedSessionStorage` restores their JWT
  without a re-login, as long as the token hasn't expired).
- Run exactly one `NLIP.Worker` replica set with Hangfire's built-in distributed locking — running
  multiple replicas is safe (Hangfire coordinates via the shared SQL Server storage) and is the
  recommended way to get Worker HA, not a hot/cold failover pair.
- SQL Server: use Always On Availability Groups (self-hosted) or Azure SQL's built-in
  active geo-replication/auto-failover groups for a secondary region.

## Recovery drill checklist

1. Restore the SQL Server backup to a clean instance.
2. Point a scratch `NLIP.API` + `NLIP.Worker` at it (separate connection string, not production).
3. Confirm `SELECT COUNT(*) FROM SubmissionQueue WHERE Status IN ('Queued','Retrying')` matches
   expectations, and that the outbox processor resumes draining them without duplicate NAICOM
   submissions (NAICOM's own idempotency/PolicyNo uniqueness is the last line of defense here —
   confirm with NAICOM what happens if the same PolicyNo is submitted twice after a failover).
4. Confirm Hangfire's dashboard shows the recurring jobs re-registered (they're declared
   idempotently in `NLIP.Worker/Program.cs` via `AddOrUpdate`, so a fresh Worker instance
   re-creates them automatically).

## Known gap

There is no automated DR drill/runbook execution in this scaffold (i.e., no scripted restore
pipeline) — the above is a manual checklist. See `docs/ROADMAP.md`.

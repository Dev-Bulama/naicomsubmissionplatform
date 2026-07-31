# User Manual (Operations / Compliance / Auditor / Read-Only roles)

## Signing in

Go to the Web dashboard URL, enter your username/password. After 5 failed attempts your account
locks for 15 minutes (both configurable by an administrator). On first login with a seeded/reset
account you'll be required to change your password.

## Dashboard

Landing page after login. Shows policy counts (total, retail, group), today's submissions,
success/failure/pending/retry/processing counts, average NAICOM response time, a 30-day submission
volume chart, NAICOM API status, top recent errors, and a recent-activity feed. Requires the
`Dashboard.View` permission.

## Policy Management

Search by policy number, customer/employer name, NAICOM ID, business type. Click **View** on any
row to open the detail screen:

- **Overview** — core policy ID, branch, agent, sum assured, premium, coverage dates
- **Beneficiaries / Members** — the Individual Life beneficiary list or Group Life member roster
- **History** — every status transition (Draft -> Active -> Updated -> Renewed -> Terminated) with
  timestamp and notes
- **Submission Timeline** — every outbox row for this policy (Create/Update/Renew/Terminate),
  its status, retry count, next scheduled attempt, and last error. A **Retry** button appears on
  any Failed/Retrying/DeadLetter row (requires `Policies.Retry` permission) — it immediately
  re-queues the submission instead of waiting for the scheduled backoff.
- **NAICOM Transactions** — the raw call log: action, success/failure, HTTP status, duration,
  timestamp, for every actual attempt (not just the current one).

## Synchronization Monitor

A live, filterable view across every policy's submissions (not scoped to one policy), grouped by
status: Queued, Processing, Completed, Failed, Retrying, DeadLetter, Cancelled. Use this screen
when you need to answer "what's currently stuck/failing" rather than "what happened to this one
policy" (that's the Policy Detail screen's job).

## What Compliance/Auditor roles can't do

Auditor and Read-Only roles are seeded with no permissions by default — an administrator must
grant `Dashboard.View`, `SyncMonitorView`, `AuditTrailView`, etc. via Roles & Permissions before
these roles can see anything (see `docs/ADMIN_MANUAL.md`). This is deliberate: the brief asks for
configurable per-role permissions, not a fixed hardcoded set.

## Getting help

If NAICOM Portal availability shows red on the dashboard, submissions will keep retrying
automatically per the backoff schedule (30s/1m/5m/15m/30m/1h) — no manual action needed until a
submission reaches DeadLetter status, at which point contact an Integration Administrator.

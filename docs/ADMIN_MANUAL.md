# Administrator Manual

## Roles

Seven fixed role *names* (`RoleNames`), each with **zero permissions by default** except
`SuperAdministrator` (seeded with every permission — see `DbInitializer.SeedRolesAsync`):
SuperAdministrator, SystemAdministrator, IntegrationAdministrator, ComplianceOfficer,
OperationsOfficer, Auditor, ReadOnlyUser. Permissions are assigned per-role via `RolePermissions` —
there is no code path that hardcodes "OperationsOfficer can do X"; everything is configurable at
runtime.

### Recommended starting permission sets

| Permission | Suggested roles |
|---|---|
| `Policies.View`, `SyncMonitor.View`, `Dashboard.View` | Operations, Compliance, Auditor, ReadOnly |
| `Policies.Create/Update/Renew/Terminate/Retry` | Operations, IntegrationAdministrator |
| `Policies.Delete` | IntegrationAdministrator only |
| `AuditTrail.View`, `Reports.View/Export` | Compliance, Auditor |
| `Settings.View/Manage`, `Users.Manage`, `Roles.Manage` | SystemAdministrator, SuperAdministrator |

Assign via the `Permissions`/`RolePermissions` tables directly (no admin UI screen for this exists
yet — see `docs/ROADMAP.md`) until a Roles & Permissions management page is built.

## Users

Seeded bootstrap account: `admin` / `ChangeMe!2026`, `MustChangePassword = true`. There is no
self-service user-creation UI in this scaffold yet; create additional users via direct DB insert
(hash the password with `PasswordHasher.Hash`, or expose a `CreateUserCommand` — the pattern for
one already exists in `LoginCommandHandler` to copy from) until a Users management screen ships.

## Settings

`/settings` (requires `Settings.View`, and `Settings.Manage` to save). Every key is described in
`docs/CONFIGURATION.md`. **Set `Naicom:Sid` and `Naicom:Secret` before any submission will
succeed** — until then every outbox row will fail at the auth step and retry per the backoff
schedule.

## Monitoring the integration

- `/hangfire` — background job dashboard (queue depth, retry schedule, failed jobs), restricted to
  SuperAdministrator/SystemAdministrator/IntegrationAdministrator.
- `/sync-monitor` — every submission across every policy, filterable by status.
- Dashboard's "NAICOM API Status" card and Top Errors list — first place to look when submissions
  are failing broadly rather than for one policy.

## Configuring the Core Application connector

The webhook endpoint (`POST /api/core-connector/policy-activated` etc.) needs a dedicated
credential issued to the Core Insurance Application — create a service account user, assign it
only the `Policies.Create` permission (or whichever specific ingest permission matches the
endpoint), and give the Core team its JWT (or wire a client-credentials flow if the Core system
can't do interactive login — not implemented in this scaffold, see ROADMAP).

## Disaster recovery / backups

See `docs/DISASTER_RECOVERY.md`.

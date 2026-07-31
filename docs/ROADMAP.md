# Roadmap — what's real, what's stubbed, what's next

This scaffold was built in one session inside a sandboxed environment with **no .NET SDK** (so
nothing here has been compiled or run) and **no network access to `portal.naicom.gov.ng`** (so the
NAICOM DTOs are a best-effort design, not a transcription of the real spec). Both constraints are
recorded here so nobody mistakes this for a verified, tested build. Read this before demoing or
deploying anything.

## Must-do before this can run at all

1. **Generate the initial EF Core migration.** No `Migrations/` folder exists.
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate -p src/NLIP.Persistence -s src/NLIP.API
   ```
   Then diff its `Up()` against `deploy/sql/schema.sql` and reconcile any drift before trusting
   either one.
2. **Verify every NAICOM DTO field/endpoint against the real spec.** See
   `src/NLIP.Integration/Dtos/README_VERIFY_AGAINST_SPEC.md` for exactly what to check and where.
   Nothing else in the codebase needs to change once the DTOs and `NaicomApiEndpoints` constants
   are corrected — that isolation was the point of the mapping-layer design.
3. **Build once with a real SDK** (`dotnet build NLIP.sln`) and fix whatever the compiler finds.
   Care was taken to get namespaces, package versions, and signatures right by hand, but "written
   carefully" is not the same guarantee as "compiled."

## Implemented and reasonably complete

- Domain: Policy aggregate with real invariants (Activate/ApplyUpdate/Renew/Terminate), domain
  events, all five tables the brief lists beyond Policies (master data, identity, queue/log tables)
- CQRS command/query set covering the full Individual Life + Group Life submission lifecycle
- Outbox pattern (transactional event -> SubmissionQueue) + Hangfire-driven retry engine with the
  exact backoff schedule from the brief (30s/1m/5m/15m/30m/1h) and dead-lettering
- NAICOM client: Polly retry + circuit breaker + timeout + bulkhead, auto token refresh/caching,
  AutoMapper-based DTO mapping (no hand-built JSON anywhere)
- JWT auth, permission-based RBAC (7 roles, ~16 permissions, admin-configurable via
  RolePermissions), account lockout, BCrypt hashing, password-complexity validator, AES-256-GCM
  encryption for secret settings
- Serilog to console/file/SQL Server with correlation IDs; audit logging behavior on every command
- Dashboard, Policy Management (search/detail/retry), Synchronization Monitor, Settings — both API
  and a working Blazor Server UI consuming it
- Docker Compose for the full stack; unit tests for domain rules/validators/security; integration
  tests booting the real API host against an in-memory Sqlite database

## Stubbed / partial — real interfaces exist, real providers don't

- **SMS/Teams/Slack notifications** — `ISmsSender`/`ITeamsNotifier`/`ISlackNotifier` exist and are
  wired into DI; the implementations log instead of calling a real provider. Swap in Termii/Twilio
  and a Teams/Slack incoming webhook client.
- **MFA** — `User.MfaEnabled`/`MfaSecret` columns and DI hooks exist; no TOTP issuance/QR
  code/verification flow is implemented.
- **Reports (Excel/PDF/CSV export, daily/weekly/monthly/quarterly/yearly builder)** — the
  underlying queries exist (dashboard stats, sync monitor); no export endpoints or report-builder
  UI. The `Reports.razor` page says so explicitly rather than faking a working screen.
- **Audit Trail screen** — `AuditLog` writes are real (every command is audited); there's no
  search/query API endpoint or grid yet, only the write side.
- **Core Application connectors beyond REST/webhook** — the abstraction point is "call the same
  MediatR commands from a different adapter"; only the webhook controller is implemented. Polling
  and RabbitMQ/Azure Service Bus/Kafka adapters are described in `ARCHITECTURE.md` but not built.
- **Session idle-timeout enforcement in the Blazor UI** — the setting (`SessionTimeoutMinutes`)
  exists; no client-side idle timer forces a logout yet.

## Explicitly out of scope (by design, not oversight)

- Every NAICOM insurance class other than Individual Life and Group Life
- Kafka connector (brief itself marks this "future")
- A hand-authored EF Core migration (see "must-do" above — auto-generating this correctly requires
  the SDK, which this environment didn't have; hand-writing migration `Designer.cs` snapshots
  would risk subtle, hard-to-detect model-snapshot mismatches)

## Suggested next three PRs

1. Generate + commit the initial migration; stand up a real SQL Server + run the full stack via
   Docker Compose; fix whatever `dotnet build`/`dotnet test` surface.
2. Verify NAICOM DTOs against the live spec; add a contract test (record/replay against a sandbox
   NAICOM environment if one is offered) so DTO drift breaks CI instead of production.
3. Wire the Audit Trail query endpoint + screen, and the Reports export endpoints — both have
   their data model and queries in place already.

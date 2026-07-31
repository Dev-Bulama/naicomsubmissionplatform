# Architecture

## Layered / Clean Architecture

```mermaid
graph TD
    subgraph Presentation
        API[NLIP.API<br/>ASP.NET Core Web API]
        WEB[NLIP.Web<br/>Blazor Server]
    end
    subgraph Application
        APP[NLIP.Application<br/>CQRS / MediatR / Validation]
    end
    subgraph Domain
        DOM[NLIP.Domain<br/>Entities, Events, Rules]
    end
    subgraph Outer[Infrastructure / Persistence / Integration]
        INFRA[NLIP.Infrastructure<br/>Serilog, JWT, Cache, Notifications, Hangfire]
        PERSIST[NLIP.Persistence<br/>EF Core, Repositories, Outbox]
        INTEG[NLIP.Integration<br/>NAICOM Client, DTOs, Polly]
    end
    WORKER[NLIP.Worker<br/>Hangfire Server]
    NAICOM[(NAICOM Portal API)]
    CORE[(Core Insurance Application)]
    DB[(SQL Server)]
    REDIS[(Redis)]

    API --> APP
    WEB -->|REST + SignalR| API
    APP --> DOM
    INFRA -.implements interfaces of.-> APP
    PERSIST -.implements interfaces of.-> APP
    INTEG -.implements interfaces of.-> APP
    PERSIST --> DB
    INFRA --> REDIS
    INTEG --> NAICOM
    WORKER --> APP
    WORKER --> PERSIST
    WORKER --> INTEG
    CORE -->|webhook / poll / queue| API
```

Dependencies point inward only: Domain has zero dependencies; Application depends only on Domain
and Shared; Infrastructure/Persistence/Integration depend on Application (implementing its port
interfaces — `IApplicationDbContext`, `INaicomApiClient`, `INotificationService`,
`IBackgroundJobScheduler`, etc.) but Application never references them back. This is what lets
NLIP.Web talk to NLIP.API purely over HTTP/SignalR while NLIP.Worker shares the same Application/
Persistence/Integration assemblies as NLIP.API without duplicating business logic.

## Submission sequence (Policy Activated -> NAICOM Create)

```mermaid
sequenceDiagram
    participant Core as Core Insurance App
    participant API as NLIP.API
    participant DB as SQL Server
    participant Worker as NLIP.Worker (Hangfire)
    participant NAICOM as NAICOM Portal

    Core->>API: POST /api/core-connector/policy-activated
    API->>API: IngestPolicyActivatedCommand (validate, map)
    API->>DB: Policy.Activate() -> PolicyActivatedEvent
    Note over API,DB: DispatchDomainEventsInterceptor publishes the event<br/>inside the same SaveChangesAsync transaction
    API->>DB: SubmissionQueue row inserted (Outbox pattern)
    API-->>Core: 202 Accepted

    loop every 15s
        Worker->>DB: SELECT due SubmissionQueue rows
        Worker->>Worker: Enqueue ProcessSubmissionCommand per row
    end

    Worker->>NAICOM: POST /api/life/individual/policy (Bearer token)
    NAICOM-->>Worker: 200 OK { naicomPolicyId }
    Worker->>DB: NaicomTransaction logged, Policy.NaicomPolicyId set,<br/>SubmissionQueue.Status = Completed
    Worker->>API: SignalR broadcast via NotificationHub
    API-->>Core: (Core polls or is notified separately — out of this platform's scope)
```

On failure, the same diagram's last three steps become: log the failed `NaicomTransaction`,
increment `SubmissionQueue.RetryCount`, set `NextAttemptAt` per the backoff schedule (30s / 1m /
5m / 15m / 30m / 1h), and clear `HangfireJobId` so the outbox poller picks it up again once due.
After `MaxRetries` (configurable, default 6) the row moves to `DeadLetter` and an
Integration Administrator is notified.

## Entity relationships (core subset)

```mermaid
erDiagram
    Policy ||--o{ PolicyBeneficiary : "has (Individual Life)"
    Policy ||--o{ GroupMember : "has (Group Life)"
    Policy ||--o{ PolicyHistory : "status changes"
    Policy ||--o{ NaicomTransaction : "API call log"
    Policy ||--o{ SubmissionQueue : "outbox/retry rows"
    Policy }o--|| Product : "product"
    Policy }o--|| Branch : "branch"
    Policy }o--o| Agent : "agent"
    Policy }o--o| Customer : "insured (Individual Life)"
    Policy }o--o| Employer : "policyholder (Group Life)"
    User ||--o{ UserRole : ""
    Role ||--o{ UserRole : ""
    Role ||--o{ RolePermission : ""
    Permission ||--o{ RolePermission : ""
```

## Key design patterns and why

| Pattern | Where | Why |
|---|---|---|
| CQRS + MediatR | `NLIP.Application/Features/*` | Separates policy-lifecycle commands from dashboard/search queries; pipeline behaviors add validation/logging/audit/perf uniformly. |
| Outbox | `SubmissionQueue` + `DispatchDomainEventsInterceptor` | A policy status change and its NAICOM submission are written in one DB transaction — a crash between "policy activated" and "submission queued" cannot happen. |
| Repository + Unit of Work | `NLIP.Persistence/Repositories` | Used for master-data CRUD (Branch/Agent/Employer/Customer/Product); Policy's own CQRS handlers use `IApplicationDbContext` directly because their queries need rich joins/projections a generic repository would only get in the way of. |
| Adapter/Ports | `INaicomApiClient`, `INotificationService`, `IBackgroundJobScheduler` | Application defines the contract; Integration/Infrastructure implement it, so swapping NAICOM's transport or the job engine never touches business logic. |
| Resilience pipeline | `NLIP.Integration.Resilience.NaicomResiliencePolicies` | Bulkhead -> Circuit Breaker -> Retry -> Timeout, in that order, wraps every NAICOM HTTP call. |
| Strategy (business type) | `NaicomApiClient`, `PolicyToNaicomMappingProfile` | Picks Individual Life vs Group Life DTOs/endpoints at runtime from `Policy.BusinessType` — adding a third Life sub-type later is one new DTO + mapping + endpoint constant, not a rewrite. |

## Connector abstraction (Core Insurance Application side)

The task brief asks for a replaceable connector to the Core Application (REST, polling, message
queue, webhook). This scaffold implements the REST/webhook variant
(`NLIP.API.Controllers.CoreConnectorController`) calling the same MediatR commands
(`IngestPolicyActivatedCommand` etc.) that a polling job or a RabbitMQ/Azure Service Bus consumer
would call. Adding a polling or queue-based connector means writing a new thin adapter (a Hangfire
recurring job, or a hosted `BackgroundService` consuming a queue) that constructs the same command
objects and sends them through the same `IMediator` — no changes needed in Domain, Application, or
Persistence. See `docs/ROADMAP.md` for what's stubbed vs. built here.

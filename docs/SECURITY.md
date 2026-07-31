# Security Checklist

| Control | Status | Where |
|---|---|---|
| JWT authentication | ✅ Implemented | `NLIP.Infrastructure.Security.JwtTokenGenerator`, `NLIP.API/Program.cs` (JwtBearer) |
| Refresh token rotation | ✅ Implemented | `RefreshTokenCommandHandler` — old token revoked + `ReplacedByToken` set on every refresh |
| Role-based authorization | ✅ Implemented | `Role`/`Permission`/`RolePermission`/`UserRole`, seeded via `DbInitializer` |
| Permission-based policy checks | ✅ Implemented | `PermissionPolicyProvider` (API) / `WebPermissionPolicyProvider` (Web) — dynamic `[Authorize(Policy = PermissionNames.X)]` |
| Password hashing | ✅ Implemented | BCrypt, work factor 12 (`PasswordHasher`) |
| Password complexity rules | ✅ Implemented | `PasswordComplexityValidator` (12+ chars, upper/lower/digit/symbol) — wire into a ChangePassword command before go-live |
| Account lockout | ✅ Implemented | `LoginCommandHandler` — configurable threshold/duration via `SettingKeys.AccountLockoutThreshold/Minutes` |
| Session timeout | ⚠️ Partial | JWT `AccessTokenMinutes` (default 30) enforces API session length; `SettingKeys.SessionTimeoutMinutes` exists but isn't yet read by any idle-timeout enforcement in the Blazor UI |
| MFA | ⚠️ Ready, not enforced | `User.MfaEnabled`/`MfaSecret` columns exist; no TOTP issuance/verification flow implemented yet |
| HTTPS enforced | ✅ Implemented | `UseHttpsRedirection()`, `RequireHttpsMetadata` in non-Development |
| Encryption at rest (secrets) | ✅ Implemented | AES-256-GCM via `SettingEncryptionService` for `SystemSettings.IsSecret` values |
| Secure secrets / environment variables | ✅ Implemented | No secrets in `appsettings.json` (placeholder tokens only); `.env.example` + Docker Compose env vars |
| Azure Key Vault ready | ✅ Ready | All secrets read via `IConfiguration` — add `AddAzureKeyVault` to `Program.cs`, no other changes |
| SQL injection protection | ✅ Implemented | EF Core parameterized queries throughout; no raw SQL string concatenation anywhere in the codebase |
| XSS protection | ✅ Implemented | Blazor's automatic output encoding; no raw HTML injection (`MarkupString`) used anywhere |
| CSRF protection | ✅ Implemented | `app.UseAntiforgery()` in `NLIP.Web`; the API is a pure JWT-bearer API (stateless, not cookie-authenticated, so classic CSRF doesn't apply to it) |
| Rate limiting | ✅ Implemented | `AspNetCoreRateLimit`, tighter limit on `/api/auth/login` (10/min) than general traffic |
| Audit logging | ✅ Implemented | `AuditLoggingBehavior` (every MediatR *Command*) + `AuditLog` entity/table |
| Correlation IDs | ✅ Implemented | `CorrelationIdMiddleware` + Serilog enrichment + `NaicomTransaction.CorrelationId`/`ApiCallLog.CorrelationId` |
| TLS to NAICOM | ✅ Implemented | `NaicomOptions.BaseUrl` is `https://`; no cert/TLS validation is bypassed anywhere in `NaicomApiClient` |
| Least-privilege DB access | ⚠️ Deployment-time | Use a SQL login scoped to `NlipDb` only in production, not `sa` (the Compose file's `sa` login is dev convenience) |

## Threat notes specific to this platform

- **NAICOM credentials (SID/Secret):** stored as `IsSecret` `SystemSettings` rows, AES-256-GCM
  encrypted at rest, never logged (`NaicomAuthTokenProvider` never writes the secret to any log
  sink — only the resulting bearer token touches `NaicomTransaction`/`ApiCallLog`, and even that
  should arguably be redacted before this goes to a real NAICOM environment; see ROADMAP).
- **Webhook endpoint (`/api/core-connector/*`):** must be secured with a dedicated
  IntegrationAdministrator-scoped credential issued to the Core Application, not shared with any
  human user account — do not point a browser-facing login at this endpoint.
- **Hangfire Dashboard:** restricted to SuperAdministrator/SystemAdministrator/
  IntegrationAdministrator roles via `HangfireDashboardAuthFilter`; do not expose `/hangfire`
  through a public-facing load balancer without this filter active.

## Before production

1. Rotate the seeded `admin` account's password immediately (it's `MustChangePassword = true` by
   default, but enforce that flow in the Web login page before go-live).
2. Generate real `Jwt:SigningKey` / `Encryption:Key` values per environment — never reuse the
   values from `.env.example` or any lower environment.
3. Run a dependency vulnerability scan (`dotnet list package --vulnerable`) before each release.
4. Pin Subresource Integrity hashes (or vendor the files locally) for the Bootstrap/Chart.js CDN
   references in `NLIP.Web/Components/App.razor` — they're unpinned in this scaffold because the
   sandbox that built it couldn't reach the CDN to compute a verified hash.

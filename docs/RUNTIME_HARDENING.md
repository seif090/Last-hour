# Runtime hardening: security, observability & request limits

This document describes the API host's runtime surface added in Sprint 00.6:
correlation ids, request logging, OpenTelemetry, security headers, forwarded
headers, CORS, secrets handling, request limits and the audit trail. All of it
is runtime/infrastructure only — no business logic depends on it and turning any
feature off degrades nothing but the concern it guards.

## Middleware order

The pipeline (from `Program.cs`) runs in this order; the order matters:

```
UseExceptionHandler          -> problem details for all unhandled errors
UseSwagger (Development)
UseForwardedHeaders          -> honor X-Forwarded-* from the proxy
UseHttpsRedirection
UseSecurityHeaders           -> HSTS/CSP/Referrer-Policy/...
UseCors                      -> CORS policy (preflight + headers)
UseResponseCompression       -> Brotli/Gzip
UseRequestLogging            -> one structured log line per request
UseAuditLogging              -> audit events for 401/403/429/5xx
UseRouting
UseRateLimiting              -> 429s
UseOutputCaching
endpoints
```

## Correlation ids

`X-Correlation-ID` is accepted on requests and echoed on responses. An incoming
header (up to `CorrelationId:MaximumIncomingLength` chars) is honored; otherwise
the OpenTelemetry trace id is reused when present, and a GUID is generated as a
last resort. The id flows through `HttpContext.Items`, the Serilog log context
and the OpenTelemetry `correlation.id` tag, so logs, traces and audit events for
one request join on a single value.

## Request logging

One structured event per request (method, path, status, elapsed ms, correlation
id, remote IP, user agent, authenticated state) written with cached
`LoggerMessage` delegates. Request bodies, query strings, headers, passwords and
tokens are **never** logged. The request id is pushed into the log context for
the duration of the request.

## OpenTelemetry

`OpenTelemetry` section enables traces and metrics with an OTLP exporter
(optional) or a console exporter for local debugging. Resource attributes set
the service name, the informational version of the entry assembly, and the
deployment environment. Instrumented sources: `LastHour.Api`,
`Microsoft.EntityFrameworkCore`, `Npgsql`, ASP.NET Core and `HttpClient`.

## Health checks

- `GET /health` — all checks
- `GET /health/live` — checks tagged `live` (the `self` check: the process
  answered)
- `GET /health/ready` — checks tagged `ready` (database, disk, memory, redis)
- `GET /` — readiness probe as JSON

Readiness failures return `503`; liveness failures return `503` only when the
process cannot answer. See `docs/INFRASTRUCTURE.md` for the check table.

## Security headers

`SecurityHeaders` section controls `X-Content-Type-Options`,
`Referrer-Policy`, `X-Frame-Options`, `Permissions-Policy`, a strict
`Content-Security-Policy` (Development overrides it to allow the Swagger UI)
and HSTS. HSTS is only emitted over HTTPS. `X-XSS-Protection` is deliberately
not sent: it is deprecated and can introduce client-side vulnerabilities; CSP
is the defense in depth.

## Forwarded headers

`ForwardedHeaders` section configures `X-Forwarded-*` handling behind a proxy.
The secure default trusts only the immediate hop (`ForwardLimit: 1`) with no
known proxies or networks; configuring a higher limit without pinning
`KnownProxies`/`KnownNetworks` is rejected at startup. HTTPS redirection and
the client address reported in request logs/audit events therefore see what
the proxy forwarded, never a spoofable header.

## CORS

`Cors` section: Development allows any origin (`AllowAnyOrigin: true`); the
base configuration lists explicit `AllowedOrigins` and forbids
`AllowAnyOrigin` in the Production environment (validated at startup).
Credentials can never be combined with any-origin access.

## Secrets

Sensitive settings never live in `appsettings.json`. The `Secrets` section
declares which configuration keys are secrets (currently
`ConnectionStrings:Postgres`); each declared key is resolved through an
`ISecretProvider` and overrides the application settings value. The default
provider reads environment variables:

| Secret name | Environment variable |
|-------------|----------------------|
| `ConnectionStrings:Postgres` | `LASTHOUR_SECRET_CONNECTIONSTRINGS_POSTGRES` |

The prefix is configurable (`Secrets:EnvironmentVariablePrefix`). Additional
providers (user secrets, key vault) can be added behind the same interface.

## Request limits

`RequestLimits` section hardens the HTTP surface: `MaxRequestBodySize` (10 MB
default), `RequestHeadersTimeout` (30 s), `KeepAliveTimeout` (130 s),
`MultipartBodyLengthLimit` (128 MB), and the slow-client minimum request body
data rate (240 bytes/s after a 5 s grace period). `null` values keep the
ASP.NET Core defaults; disabling the section restores them.

## Audit trail

Security-relevant HTTP outcomes (401, 403, 429, 5xx) are written to a dedicated
audit log file (`logs/last-hour-audit-.log`, compact JSON, daily rollover)
through the `IAuditLogger` abstraction. Audit events are deliberately sparse —
action, outcome, status, correlation id, client address — and never carry
bodies, query strings or payloads. The audit log is intentionally separate from
the operational logs so it cannot be sampled or rotated away as easily.

## Configuration validation

Every options section (`Postgres`, `Outbox`, `HealthChecks`, `Cqrs:Performance`,
`DatabaseInitializer`, `ResponseCompression`, `CorrelationId`, `OpenTelemetry`,
`SecurityHeaders`, `ForwardedHeaders`, `Cors`, `Secrets`, `RequestLimits`,
`AuditLogging`) is bound with `ValidateOnStart`. Misconfiguration fails the
process at startup instead of surfacing at runtime.

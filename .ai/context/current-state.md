# Current state — live status

> Maintained for AI engineering context. Do not edit by hand; regenerated at
> each sprint close.

## Sprint 00.5 — Infrastructure Foundation

**Status: complete.** Verified against the sprint exit criteria.

### What shipped

- Persistence: `LastHourDbContext` with snake_case/UTC/decimal conventions,
  `AuditInterceptor`, `SoftDeleteInterceptor`, `IUnitOfWork`, repository base,
  strongly typed ID value converters.
- EF Core 9 options wiring via `IDbContextOptionsConfiguration<LastHourDbContext>`
  (scoped registration; resolved the action-less `AddDbContext` gap).
- Outbox: `OutboxMessage` entity + `OutboxProcessor` hosted service (disabled by
  `Outbox:Enabled`).
- Seeding: `IDatabaseSeeder`, `SeederExecutor`, `seeding_history` dedupe table,
  `DatabaseInitializer` hosted service gated on `Enabled`/Development/relational.
- Health checks: postgres, disk, memory, redis (`HealthChecks` options section).
- DI surface: `AddLastHourInfrastructure(services, configuration, moduleAssemblies)`
  and `AddCqrs` (MediatR + pipelines), options binding with `ValidateOnStart`.
- API wiring: `AddLastHourApi` calls `AddLastHourInfrastructure`; health endpoints
  live under `/health`, `/health/live`, `/health/ready`, `/`.
- Appsettings: `ConnectionStrings:Postgres`, `Outbox`, `DatabaseInitializer`,
  `HealthChecks` sections (Development configures Redis `localhost:6379`).
- Tests: `LastHourApiFactory` shared host for API integration tests; infra unit
  tests for options, seeding, outbox, health checks, DI validation
  (`ValidateScopes`).

### Quality gates

- Build: 0 warnings / 0 errors (`TreatWarningsAsErrors`, StyleCop + IDE analyzers).
- `dotnet format --verify-no-changes` exits 0.
- Tests: **230 passing** — SharedKernel 45, Infrastructure 144, Api 41.

### Known limitations

- No business modules wired yet (module assemblies still empty; no entities in
  modules to configure).
- `DatabaseInitializer` migrations/seeding require a reachable database; disabled
  in test hosts by design.
- Redis health check only participates when `HealthChecks:Redis:ConnectionString`
  is configured (empty by default in production appsettings).

## Prior sprints

### Sprint 00.1 — Foundation

Empty class libraries + xUnit scaffolds for the full solution layout, quality
gates (CPM, StyleCop, warnings-as-errors, nullable), Serilog structured logging,
API versioning, output caching, rate limiting, correlation ID middleware,
problem details, Swagger, Docker compose stack (API, PostgreSQL, Redis, Seq,
PgAdmin), CI workflows. See `docs/ARCHITECTURE_AUDIT.md`.

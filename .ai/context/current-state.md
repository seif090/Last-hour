# Current State

> Live status of the LastHour backend repository. Updated at the end of each
> sprint. Do not edit historical sections — append a new entry instead.

## Sprint 00.1 — Solution foundation (in progress → completed)

### Scope

Initialize the repository as a production-ready enterprise solution skeleton.
No business logic, no entities, no authentication, no module wiring.

### Delivered

- Solution: `LastHour.Backend.sln` (35 projects).
- Top-level folders: `.ai/`, `docs/`, `src/`, `tests/`, `docker/`,
  `deploy/`, `scripts/`, `.github/`.
- Building blocks (`src/BuildingBlocks/`):
  - `LastHour.BuildingBlocks.Architecture`
  - `LastHour.BuildingBlocks.SharedKernel`
  - `LastHour.BuildingBlocks.Application`
  - `LastHour.BuildingBlocks.Infrastructure`
  - `LastHour.BuildingBlocks.Contracts`
- Modules (`src/Modules/`): `Identity`, `Merchant`, `Catalog`, `Inventory`,
  `Offers`, `Ordering`, `Payments`, `Notifications`, `Analytics`,
  `Administration`, `Platform` (11 modules).
- API host: `src/Api/LastHour.Api` (controllers + OpenAPI, `/` health probe).
- Tests: xUnit project for every src project (18 test projects).
- Reference graph: building blocks depend downward only; each module
  references `Application + Architecture + SharedKernel + Contracts`; the
  API references the building blocks + every module. No cycles.
- Root artifacts (already present, reused):
  `Directory.Build.props`, `global.json`, `.editorconfig`, `.gitignore`,
  `NuGet.config`.
- Root artifact added in this sprint: `README.md`.
- `.github/workflows/build.yml` for CI (restore + build + test).
- Stub folders `docker/`, `deploy/`, `scripts/`, `docs/` with `.gitkeep`
  placeholders.

### Architecture decisions

1. **Solution name** is `LastHour.Backend.sln` — naming reserves `LastHour.*`
   for non-backend surfaces (mobile, web) introduced in later sprints.
2. **Project naming** follows `LastHour.<Layer>[.<Module>]`. This keeps
   fully-qualified type names unambiguous (`LastHour.Modules.Payments.Pay`
   vs. `LastHour.Api.Pay`).
3. **Reference direction is strict and one-way**. Architecture depends on
   nothing. SharedKernel depends on Architecture. Application depends on
   Architecture + SharedKernel. Infrastructure depends on Application (and
   transitively the rest). Contracts depends on Application (cross-process
   boundary types). Each Module depends on Application + Architecture +
   SharedKernel + Contracts. The API host is the only place all
   dependencies are visible — it is the composition root.
4. **No Module → Module references** at this stage. Module-to-module
   communication will use integration events or contracts in a later
   sprint.
5. **No Module → Infrastructure references** either. Modules stay pure;
   infrastructure adapters live in the API host or in a future
   `LastHour.<Module>.Infrastructure` project.
6. **No Module → Contracts cross-references** at this stage. Contracts is
   reserved for stable inter-module / inter-service payloads.
7. **`TreatWarningsAsErrors=true`** is kept. The default xUnit + webapi
   templates compile cleanly under this flag, which we want — it keeps
   the bar for future code honest.
8. **Central package management** is left enabled (`ManagePackageVersionsCentrally=true`),
   even though no packages are pinned yet — this will be filled in when
   the first real NuGet dependencies land (e.g. MediatR, EF Core, Serilog).
9. **`global.json` pins SDK 9.0.311** with `latestFeature` roll-forward,
   matching the installed 9.0.x SDKs and keeping the door open for
   automatic patch upgrades.
10. **API host keeps controllers + OpenAPI** but the weather demo is
    removed. The only HTTP surface today is `GET /` returning a JSON
    readiness payload — foundation only.
11. **CI workflow** is `.github/workflows/build.yml` running restore +
    build + test on Ubuntu. It does not deploy yet.

### Warnings / errors

- Build emits 0 warnings, 0 errors.
- No fixes were necessary: the existing `Directory.Build.props`
  (nullable on, treat-warnings-as-errors on, analyzers at
  `latest-recommended`) was already strict enough that the bare xUnit
  + webapi + classlib templates built green on first attempt.

### Files of interest

- `LastHour.Backend.sln` — solution manifest.
- `Directory.Build.props` — shared project properties (target framework,
  nullable, analyzers, central package management).
- `global.json` — SDK pin.
- `.editorconfig` — code style.
- `NuGet.config` — single `nuget.org` feed.
- `.github/workflows/build.yml` — CI.
- `README.md` — repo overview.
- `src/Api/LastHour.Api/Program.cs` — minimal composition root + `/`
  endpoint.

### Next sprint (Sprint 00.2) — not started

- Introduce `Directory.Packages.props` with the first set of centralized
  package versions (MediatR, EF Core, FluentValidation, Serilog, etc.).
- Define shared kernel primitives (Result, Error, ValueObject, Entity,
  AggregateRoot, IDomainEvent).
- Define Application abstractions (ICommand, IQuery, ICommandHandler,
  IQueryHandler, IIntegrationEvent).
- Define Contracts envelopes (Command, Event, Query DTOs).
- Add ArchUnit tests in `LastHour.BuildingBlocks.Architecture.Tests` that
  enforce the reference graph.

## Sprint 00.1 — Enterprise foundation hardening (completed 2026-08-06)

### Scope

Second pass over the solution skeleton: validate Clean Architecture and raise
the repository to enterprise standards. No business logic, no breaking
changes, no rewrites of working code.

### Delivered

- **Solution structure fixed**: rewrote `LastHour.Backend.sln` — solution
  folders previously reused one GUID (all modules nested under Identity;
  `tests/Api` misplaced). Each module now has its own folder under `src/` and
  `tests/`; 35 projects confirmed via `dotnet sln list`.
- **Central Package Management** extended and pinned in
  `Directory.Packages.props`: `Serilog.AspNetCore 10.0.0`,
  `Serilog.Settings.Configuration 10.0.1`, `Serilog.Sinks.Seq 9.1.0`,
  `StyleCop.Analyzers 1.1.118`; test stack bumped to `Microsoft.NET.Test.Sdk
  18.8.1`, `xunit 2.9.3`, `xunit.runner.visualstudio 3.1.5`, `coverlet.collector
  10.0.1`. All versions exact.
- **Layered `Directory.Build.props`**: root (framework, nullable,
  warnings-as-errors, analyzers, CPM) + `src/` (StyleCop package +
  `stylecop.json`) + `tests/` (`IsTestProject`, `IsPackable=false`), with the
  root file explicitly imported (MSBuild imports only the nearest
  `Directory.Build.props`). `GenerateDocumentationFile=true` enables the
  IDE0005 gate; CS1591 stays suppressed.
- **`stylecop.json`** created (repo root): documentation rules off, file
  headers off, `newlineAtEndOfFile=require`, System usings first, usings
  outside namespace. Note: StyleCop 1.1.118 schema requires
  `documentInterfaces` as a boolean (the `"none"` string is master-only) —
  set to `false`.
- **`.editorconfig`** rewritten: naming conventions (I-prefix interfaces,
  PascalCase types/members/consts, `_camelCase` private fields, camelCase
  locals/parameters) and 6 IDE rules enforced at build (warning→error):
  IDE0005, IDE0011, IDE0036, IDE0130, IDE0161, IDE0240.
- **`NuGet.config`**: `packageSourceMapping` added (all packages bound to
  `nuget.org`); indentation fixed.
- **Placeholders removed**: 16 `Class1.cs` template files deleted from `src`.
- **API host** (`Program.cs`): Serilog bootstrap (`CreateBootstrapLogger` +
  `UseSerilog` from config, `UseSerilogRequestLogging`) and health checks
  `/health`, `/health/live`, `/health/ready`. `Program` partial declared as an
  empty class body (keeps `WebApplicationFactory<Program>`, satisfies StyleCop
  SA1106/layout rules).
- **appsettings**: Serilog section (Console + rolling File; Seq opt-in via
  compose env), Development connection-string placeholders.
- **Docker** (`docker/`): multi-stage non-root `Dockerfile` (publish on
  `sdk:9.0`, runtime on `aspnet:9.0` as `app` user, `curl` for health checks,
  writable `/app/logs`), `docker-compose.yml` (api + postgres 16 + redis 7 +
  seq + pgadmin; named volumes; bridge network; `restart: unless-stopped`;
  health checks), `.env.example` (placeholders), `.dockerignore`. `.gitkeep`
  placeholders removed from `docker/` and `docs/`.
- **CI** (`.github/workflows/build.yml`): `permissions: contents: read`,
  `CI: true`, SDK pinned via `global.json`, `dotnet format --verify-no-changes`
  gate, TRX test results + API publish artifacts.
- **Documentation**: `docs/ARCHITECTURE_AUDIT.md` (this audit); `README.md`
  updated.

### Architecture decisions

1. StyleCop applies to `src` only; test projects inherit SDK analyzers + code
   style but not StyleCop.
2. No XML file headers (`file_header_template = unset`, SA1633 off); all
   StyleCop documentation rules disabled.
3. `GenerateDocumentationFile=true` with `NoWarn CS1591` — enables the unused-
   using build gate without requiring doc comments.
4. Serilog uses standard sinks only (Console, File, optional Seq via env); no
   custom sinks.
5. Health-check split: `/health` (all), `/health/live` (liveness), `/health/ready`
   (readiness); check set intentionally empty until infrastructure lands.
6. Test packages moved to latest stable and verified green under
   warnings-as-errors.
7. `Program` partial as an empty class body (not `Program;`).
8. Docker: non-root runtime, single source of truth for env via `.env.example`,
   health checks on every service where applicable.

### Warnings / errors

- `dotnet build -c Release`: 0 warnings, 0 errors (35 projects).
- `dotnet test`: exit 0 (suites empty; host + runner functional).
- `dotnet format --verify-no-changes`: exit 0.
- `docker compose config`: valid.
- During the pass: `TargetFramework ''` (Directory.Build.props shadowing) and
  StyleCop SA0002 (`documentInterfaces: "none"` invalid on 1.1.118) were
  encountered and resolved.

### Files of interest

- `LastHour.Backend.sln` — rewritten (folder nesting fixed).
- `Directory.Build.props`, `src/Directory.Build.props`, `tests/Directory.Build.props`.
- `Directory.Packages.props` — complete pinned version table.
- `NuGet.config`, `.editorconfig`, `stylecop.json`.
- `src/Api/LastHour.Api/Program.cs`, `appsettings.json`,
  `appsettings.Development.json`.
- `docker/Dockerfile`, `docker/docker-compose.yml`, `docker/.env.example`,
  `docker/.dockerignore`.
- `.github/workflows/build.yml`.
- `docs/ARCHITECTURE_AUDIT.md`, `README.md`.

### Next sprint (Sprint 00.2) — not started

- Shared kernel primitives (Result, Error, ValueObject, Entity, AggregateRoot,
  IDomainEvent).
- Application abstractions (ICommand, IQuery, handlers, IIntegrationEvent) and
  Contracts envelopes.
- Architecture enforcement tests (reference graph + naming) in
  `LastHour.BuildingBlocks.Architecture.Tests`.
- First infrastructure wiring (EF Core/Dapper, Redis) — populates health
  checks and connection strings.
- Follow-ups: `packages.lock.json` (deterministic restore), CI NuGet caching,
  execute the Docker image build, plan the SDK 10.0.x upgrade.


# Work Log: 2026-06-10 — feature/application_layer

## Session Goal
Build out the Application layer's CQRS/MediatR pipeline (logging, validation, unit-of-work
behaviors) and the Infrastructure layer (EF Core + PostgreSQL persistence for `Order`, Marten
event store for domain-event audit logging, DI wiring), and complete ADR-005's pending
Marten/PostgreSQL/Docker Compose setup.

## Completed

### Domain (minimal addition)
- Added `src/Domain/Common/IHasDomainEvents.cs`; `AggregateRoot<TId>` now implements it
  (no member changes — `DomainEvents`/`ClearDomainEvents()` already existed). See ADR-008.
  `tests/Domain.Tests` still 23/23, `Domain.csproj` still has no new package references.

### Application
- Removed placeholder `src/Application/Class1.cs` and `tests/Application.Tests/UnitTest1.cs`.
- Added `src/Application/Common/Messaging/`: `ICommand`, `ICommand<TResponse>`,
  `IQuery<TResponse>` CQRS marker interfaces over MediatR's `IRequest`/`IRequest<TResponse>`.
- Added `src/Application/Common/Logging/LoggingExtensions.cs` — structured
  `[LogLevel][Layer][Incremental]` log-id helper (Application = 2xxx).
- Added `src/Application/Common/Behaviors/`:
  - `LoggingBehavior<TRequest,TResponse>` — logs request start (2201), success (2202), and
    exceptions (2203, via `LogErrorWithId`) using `typeof(TRequest).Name`.
  - `ValidationBehavior<TRequest,TResponse>` — runs all registered `IValidator<TRequest>`,
    throws `FluentValidation.ValidationException(failures)` on failure.
  - `UnitOfWorkBehavior<TRequest,TResponse>` — calls `IUnitOfWork.SaveChangesAsync` after
    `next()` only when `request is ICommand or ICommand<TResponse>`.
- Added `src/Application/DependencyInjection.cs` (`AddApplication`): MediatR registration +
  the three behaviors (Logging → Validation → UnitOfWork, outermost first) +
  `AddValidatorsFromAssembly` + `AddAutoMapper(_ => { }, assembly)`.
- Added `tests/Application.Tests/Common/Behaviors/`: `LoggingBehaviorTests`,
  `ValidationBehaviorTests`, `UnitOfWorkBehaviorTests` (7 tests, Moq-based, `public sealed
  record` test request/command/query types per Moq+Castle DynamicProxy constraints).

### Infrastructure
- Removed placeholder `src/Infrastructure/Class1.cs`.
- Added packages to `src/Infrastructure/Infrastructure.csproj`: `Microsoft.EntityFrameworkCore`,
  `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Relational`,
  `Npgsql.EntityFrameworkCore.PostgreSQL`, `Marten`, `MediatR` (all `10.0.9`/`10.0.2`/`9.7.1`/
  `14.1.0` as resolved — see ADR-009 for exact versions), plus `ProjectReference` to
  `Domain.csproj` and `<InternalsVisibleTo Include="Infrastructure.Tests" />`.
- Added `src/Infrastructure/Common/Logging/LoggingExtensions.cs` — duplicate of Application's
  helper (Infrastructure = 4xxx); intentional, documented duplication (Infrastructure cannot
  depend on Application).
- Added `src/Infrastructure/Persistence/`:
  - `ApplicationDbContext` (implements `IUnitOfWork`; `SaveChangesAsync` collects domain events
    via `IHasDomainEvents` before save, clears after save, then dispatches each to
    `IEventStore.AppendAsync` (Marten) and `IPublisher.Publish` (MediatR)).
  - `Configurations/OrderConfiguration.cs`, `Configurations/OrderItemConfiguration.cs` —
    Fluent API mapping for `Order`/`OrderItem` per ADR-007 (value-converted strongly-typed IDs,
    owned types for `Address`/`Money`, backing-field access for `Order._items`, ignored computed
    properties `TotalAmount`/`Subtotal`/`DomainEvents`).
  - `Repositories/OrderRepository.cs` — `IOrderRepository` over `ApplicationDbContext`
    (`GetByIdAsync` with `Include(o => o.Items)`, `AddAsync`, `ExistsAsync`); removed
    unnecessary `async`/`await` per AsyncFixer01.
  - `ApplicationDbContextFactory` (`IDesignTimeDbContextFactory<ApplicationDbContext>`) +
    `EventStore/NullEventStore.cs` + `Persistence/NullPublisher.cs` — design-time stand-ins so
    `dotnet ef` works without full DI/Marten/a live DB.
  - `Persistence/Migrations/` — generated `InitialCreate` migration (Orders + OrderItems
    tables, FK + index).
- Added `src/Infrastructure/EventStore/`: `IEventStore`, `MartenEventStore` (appends domain
  events to a Marten stream keyed by `ResolveStreamId` — switch over the four
  `Domain.Orders.Events.*` types, returning `OrderId.Value`; logs via `LogInformationWithId`
  4401).
- Added `src/Infrastructure/DependencyInjection.cs` (`AddInfrastructure`): `AddDbContext`
  (Npgsql), `IUnitOfWork`/`IOrderRepository` registration, `AddMarten` with
  `DatabaseSchemaName = "event_store"` + `UseLightweightSessions`, `IEventStore` registration.

### Docker / Configuration / API wiring
- Added `docker-compose.yml` (repo root) — single `postgres:17` service, `dotnet_clean_boilerplate`
  database, dev-only credentials (`app_user`/`app_password`), persisted volume.
- `src/API/appsettings.json` — added empty `ConnectionStrings:Database` placeholder (production
  via `ConnectionStrings__Database` env var).
- `src/API/appsettings.Development.json` — added local Postgres connection string matching
  `docker-compose.yml` (gitignored, not in `git status`).
- `src/API/Program.cs` — added `builder.Services.AddApplication()` and
  `builder.Services.AddInfrastructure(builder.Configuration)` before `AddOpenApi()`.
- `src/API/API.csproj` — added `ProjectReference`s to `Application.csproj`/`Infrastructure.csproj`
  and `Microsoft.EntityFrameworkCore.Design` (required for `dotnet ef`'s startup-project check).

### Tooling / Migration
- Added `.config/dotnet-tools.json` — local tool manifest pinning `dotnet-ef` 10.0.9.
- Generated EF Core `InitialCreate` migration via:
  `dotnet ef migrations add InitialCreate --project src/Infrastructure/Infrastructure.csproj
  --startup-project src/API/API.csproj --output-dir Persistence/Migrations`
- Did **not** run `dotnet ef database update` (requires `docker-compose up -d`) — manual
  follow-up.

### New test project
- Added `tests/Infrastructure.Tests/Infrastructure.Tests.csproj` (mirrors `Domain.Tests`/
  `Application.Tests` conventions; `Microsoft.EntityFrameworkCore.Sqlite` 10.0.9, `Moq`
  4.20.72, `xunit` 2.9.3). Added to `DotnetCleanCodeBoilerplate.slnx`.
- `TestSupport/SqliteApplicationDbContextFactory.cs` — open `SqliteConnection("DataSource=:memory:")`
  + `EnsureCreated()`, `CreateContext(eventStore?, publisher?)` with Moq defaults.
- `Persistence/Configurations/OrderConfigurationTests.cs` (3 tests) — round-trip persistence
  with items, owned-type `Address` columns via raw ADO query, ignored computed properties.
- `Persistence/Repositories/OrderRepositoryTests.cs` (4 tests).
- `Persistence/ApplicationDbContextTests.cs` (2 tests) — domain-event collection/dispatch via
  Moq `IEventStore`/`IPublisher` (`Times.Exactly(2)` for `AddItem` + `Place`).
- `EventStore/MartenEventStoreTests.cs` (1 test) — `MartenEventStore.ResolveStreamId` via
  `InternalsVisibleTo`; header notes a live-Postgres Marten round-trip is out of scope.
- `DependencyInjectionTests.cs` (1 test) — `AddInfrastructure` resolves
  `ApplicationDbContext`/`IUnitOfWork`/`IOrderRepository`/`IEventStore`; registers a
  `Mock.Of<IPublisher>()` standing in for Application's registration.

## Verification
- `dotnet build` — succeeds, 0 errors, 0 new warnings (4 pre-existing CA5394 warnings in
  `src/API/Program.cs`, unrelated).
- `dotnet test` — all pass: `Domain.Tests` 23/23, `Application.Tests` 7/7 (the placeholder
  `UnitTest1.cs` was removed, leaving the 7 new behavior tests), `Infrastructure.Tests` 11/11.
- `dotnet ef migrations add InitialCreate ...` — succeeded using
  `ApplicationDbContextFactory`; generated migration files compile.

## Decisions Made
- ADR-008: `IHasDomainEvents` on `AggregateRoot<TId>`.
- ADR-009: hybrid persistence — EF Core/PostgreSQL (`public` schema) for `Order` + Marten
  (`event_store` schema) for domain-event audit log; single Postgres instance via
  `docker-compose.yml`; resolved package versions recorded.
- ADR-010: pipeline behaviors live in `Application/Common/Behaviors`; `ValidationBehavior`
  throws `FluentValidation.ValidationException` (mapping to `Result<T>` 400 deferred to API
  layer — added to `pending_decisions.md`).
- Documented the `LoggingExtensions` duplication between `Application.Common.Logging` and
  `Infrastructure.Common.Logging` as intentional and tracked.

## Next Steps (Not Started)
- `src/Application/Orders/` — CQRS commands/queries/handlers/DTOs/AutoMapper profiles for the
  `Order` aggregate (CreateOrder, AddOrderItem, PlaceOrder, ShipOrder, CancelOrder, etc.).
- `src/API/` — Minimal API endpoints mapping to those commands/queries via `Result<T>`.
- API-layer exception handling: map `FluentValidation.ValidationException` to `Result<T>` 400
  (see `pending_decisions.md`).
- Manual: `docker-compose up -d` then
  `dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj`
  to create the `public`/`event_store` schemas; confirm Marten creates `event_store` on first
  use.
- ARCHITECTURE.md, CONTRIBUTING.md.

---

## Post-Review Fixes (same session)

A code review of the above pass raised three changes plus one pre-existing observation:

1. **Named per-event logging methods (ADR-011)**: `LoggingBehavior` and `MartenEventStore`
   were calling the generic `LogInformationWithId`/`LogErrorWithId` helpers directly with
   inline log IDs/templates. Added `LogHandlingRequest`, `LogHandledRequest`,
   `LogUnhandledRequestException` to `Application/Common/Logging/LoggingExtensions.cs`, and
   `LogAppendedDomainEvent` to `Infrastructure/Common/Logging/LoggingExtensions.cs`. Updated
   `LoggingBehavior`/`MartenEventStore` to call these instead. Documented the convention as a
   rule in `user_preference.md` ("Call-Site Convention" under Logging Strategy).

2. **Secrets handling (ADR-013)**:
   - `docker-compose.yml` no longer hardcodes `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD`;
     it now reads `${POSTGRES_DB}`, `${POSTGRES_USER}`, `${POSTGRES_PASSWORD}`,
     `${POSTGRES_PORT:-5432}` from a gitignored `.env` (template: new committed `.env.example`).
   - `.gitignore` now also excludes `.env`.
   - `ApplicationDbContextFactory` no longer has a hardcoded `DefaultConnectionString`
     fallback (it duplicated the same dev credentials); it now adds
     `.AddEnvironmentVariables()` and throws a clear `InvalidOperationException` with setup
     instructions if `ConnectionStrings:Database` is missing.
   - `tests/Infrastructure.Tests/appsettings.json` (new, committed) holds the placeholder
     connection string (`Host=localhost;...;Username=test;Password=test`) previously inlined
     in `DependencyInjectionTests.cs`; the test now loads it via `ConfigurationBuilder` +
     `AddJsonFile`. Added `<Content Include="appsettings.json" CopyToOutputDirectory=
     "PreserveNewest" />` to `Infrastructure.Tests.csproj`. No `appsettings.Development.json`
     needed for this project — the value isn't a real credential (never connects).

3. **`Extensions/ServiceExtension.cs` convention (ADR-012)**: Moved
   `src/Application/DependencyInjection.cs` → `src/Application/Extensions/ServiceExtension.cs`
   (class renamed `DependencyInjection` → `ServiceExtension`, same `AddApplication` method) and
   `src/Infrastructure/DependencyInjection.cs` →
   `src/Infrastructure/Extensions/ServiceExtension.cs` (same pattern, `AddInfrastructure`).
   Updated `src/API/Program.cs` (`using Application.Extensions;` /
   `using Infrastructure.Extensions;`) and
   `tests/Infrastructure.Tests/DependencyInjectionTests.cs`. Documented the convention in
   `user_preference.md` under a new "Dependency Injection Registration" subsection.

4. **Pre-existing naming/style diagnostics (not fixed, recorded as pending decision)**:
   IDE flagged `UnitOfWorkBehavior.cs` for IDE1006 ("Prefix '_' is not expected" on
   `_unitOfWork`), IDE0290 ("Use primary constructor"), and CA1062 ("validate `next` is
   non-null"). All three are `suggestion`-severity and reflect either a long-standing
   `.editorconfig`/`user_preference.md` vs. actual-codebase convention mismatch (`_camelCase`
   private fields are used everywhere) or a check that contradicts "don't validate what can't
   happen" (MediatR guarantees `next` is non-null). Not changed — see `pending_decisions.md`
   for the recommended resolution (update `.editorconfig` + `user_preference.md` wording to
   match existing `_camelCase` usage, rather than rewriting the codebase).

### Verification (post-review)
- `dotnet build` — 0 errors, 0 new warnings (same 4 pre-existing CA5394).
- `dotnet test` — `Domain.Tests` 23/23, `Application.Tests` 7/7, `Infrastructure.Tests` 11/11.

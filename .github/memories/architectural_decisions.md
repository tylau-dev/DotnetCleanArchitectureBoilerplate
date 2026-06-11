# Architectural Decisions

## ADR-001: Clean Architecture + DDD + CQRS Pattern

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Building a professional-grade .NET boilerplate project that serves as a reference for future applications.

### Decision
Implement Clean Architecture with Domain-Driven Design (DDD) and Command Query Responsibility Segregation (CQRS) patterns.

### Rationale
- **Clean Architecture**: Provides clear separation of concerns across Domain, Application, Infrastructure, and API layers
- **DDD**: Enables modeling complex business logic through aggregates, entities, value objects, and domain events
- **CQRS**: Separates read (Query) and write (Command) concerns for scalability and maintainability

### Consequences
- Increased initial development time due to layer separation
- Improved testability and maintainability
- Better alignment with enterprise patterns
- Easier to adapt for various business domains

---

## ADR-002: Standardized Result<T> API Response Wrapper

**Date**: May 15, 2026  
**Status**: Accepted

### Context
API endpoints need consistent response handling for success, validation errors, and server errors.

### Decision
Use standardized `Result<T>` wrapper for all API responses with properties: Success, Data, Errors, StatusCode.

### Rationale
- Provides consistent API contract across all endpoints
- Simplifies client-side error handling
- Enables centralized error serialization
- Supports validation error details

### Consequences
- All endpoints must conform to Result<T> pattern
- Requires custom response serialization
- Better client predictability

---

## ADR-003: .NET 10 as Target Framework

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Choosing between .NET 10 (latest, cutting-edge) vs .NET 8 LTS (stable, long-term support).

### Decision
Use .NET 10 as the target framework.

### Rationale
- Latest language features and performance improvements
- Demonstrates modern C# capabilities (C# 13)
- Still on LTS-adjacent release cycle with regular updates
- Boilerplate serves as learning resource; newer is better for that purpose

### Consequences
- Must maintain updated with .NET 10 releases
- Some enterprise constraints may limit adoption
- Access to latest language features

---

## ADR-004: Separate Unit and Integration Test Projects

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Tests need clear organization with equal emphasis on unit tests (domain logic) and integration tests (CQRS handlers).

### Decision
Create two separate test projects:
- `tests/Domain.Tests/` - Unit tests for domain logic
- `tests/Application.Tests/` - Integration tests for application handlers

### Rationale
- Clear separation of test responsibilities
- Enables focused test execution
- Better organization for complex test fixtures
- Easier to maintain and extend

### Consequences
- More test projects to maintain
- Clear build and test execution paths
- Better test organization and discovery

---

## ADR-005: Marten for Event Sourcing with Docker Support

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Event sourcing is core to the infrastructure layer. Two primary options existed: Marten (PostgreSQL-based) and EventStore.Client (dedicated).

### Decision
Use **Marten** with PostgreSQL for event sourcing. Implement Docker + Docker Compose for local development using official PostgreSQL image.

### Rationale
- Marten simplifies event sourcing integration with PostgreSQL
- Docker Compose enables consistent local development environment
- PostgreSQL official image ensures stability and security updates
- Reduces operational complexity compared to dedicated event store
- Better integration with Entity Framework Core for standard ORM needs
- Excellent support for CQRS patterns and domain events

### Implementation Details
- **Event Store**: Marten
- **Database**: PostgreSQL (official Docker image)
- **Orchestration**: Docker Compose for local development
- **Connection**: configurable via appsettings.Development.json
- **Infrastructure**: Docker + docker-compose.yml in project root

### Consequences
- PostgreSQL dependency required for local and production environments
- Docker required for local development (documented in CONTRIBUTION.md)
- Marten package added to Directory.Build.props
- Infrastructure layer manages Marten configuration and migrations

---

## ADR-006: Development Practices Documentation

**Date**: May 15, 2026  
**Status**: Accepted

### Decision
Comprehensive development practices documented in `.github/user_preference.md` covering:
- Code standards and organization
- Architecture enforcement
- Testing strategy
- Configuration management
- Git workflow
- Quality gates and validation

### Rationale
- Ensures consistency across development
- Provides onboarding reference
- Documents expectations clearly
- Prevents architectural drift

### Consequences
- Clear guidelines for implementation
- Must maintain consistency with documented practices

---

## ADR-007: Persistence-Aware Domain Entities — No Separate Persistence Model

**Date**: June 10, 2026
**Status**: Accepted

### Context
The Domain layer must remain free of external dependencies (no EF Core, MediatR, etc.) per
Clean Architecture rules, while Infrastructure will later need to map these types directly to
an EF Core `DbContext`. Two approaches were considered:
1. Pure domain models + a separate set of EF Core persistence/DTO models with a mapping layer.
2. A single set of domain types, shaped so EF Core's Fluent API can map them directly, with no
   mapper layer.

### Decision
Use approach 2: Domain entities and value objects in `src/Domain` are designed to be
**persistence-aware by shape** while remaining **dependency-free**. Concretely, for the new
"Order Management" example domain (`src/Domain/Orders`):
- `Order` and `OrderItem` expose private/protected parameterless constructors reserved for EF
  Core materialization (`Entity<TId>`, `AggregateRoot<TId>` also provide protected parameterless
  constructors).
- Collection navigations (`Order.Items`) are backed by a private `List<OrderItem>` field and
  exposed as `IReadOnlyCollection<OrderItem>`.
- Strongly-typed IDs (`OrderId`, `OrderItemId`, `CustomerId`, `ProductId`) are `readonly record
  struct` wrappers around `Guid` with a `New()` factory — EF 8+ value-converts these
  automatically without any Domain-side EF awareness.
- Value objects (`Money`, `Address`) are simple immutable classes with straightforward
  constructors so Infrastructure can configure them as EF "owned types" later.

### Rationale
- Avoids duplicating the model and writing/maintaining a mapping layer between domain and
  persistence representations — a common source of drift and boilerplate.
- Keeps `Domain.csproj` free of any package references beyond what `Directory.Build.props`
  injects (verified via `dotnet list package`).
- EF Core 8+'s constructor binding and private-member access make it practical to map
  encapsulated, behavior-rich entities directly without exposing public setters.

### Consequences
- Infrastructure's future EF Core configuration must use `IEntityTypeConfiguration<T>` with
  Fluent API (backing-field access for `_items`, owned-type configuration for `Money`/`Address`,
  value converters for the strongly-typed ID structs) — no Domain-side changes required.
- Domain entities carry a couple of EF-oriented constructs (private parameterless ctors,
  `= null!` defaults for owned-type properties) that exist solely to support future
  materialization; this is an accepted, documented trade-off in exchange for avoiding a
  parallel persistence model.
- If a future bounded context genuinely needs a divergent persistence shape, that context can
  introduce its own mapper without affecting this convention elsewhere.

---

## ADR-008: `IHasDomainEvents` Marker Interface on `AggregateRoot<TId>`

**Date**: June 10, 2026
**Status**: Accepted

### Context
Infrastructure's `ApplicationDbContext.SaveChangesAsync` override needs to collect and dispatch
domain events from all tracked aggregates before/after persistence, without depending on the
generic `TId` parameter of `AggregateRoot<TId>` (EF Core's `ChangeTracker.Entries<T>()` requires a
single, non-generic type to query against).

### Decision
Add `src/Domain/Common/IHasDomainEvents.cs`:
```csharp
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```
`AggregateRoot<TId>` now implements `IHasDomainEvents` (no member changes — `DomainEvents` and
`ClearDomainEvents()` already existed on the base class).

### Rationale
- Lets `ApplicationDbContext.SaveChangesAsync` use
  `ChangeTracker.Entries<IHasDomainEvents>().SelectMany(e => e.Entity.DomainEvents)` to collect
  events across all aggregate types in a single query.
- Purely additive — no existing members changed, no new dependencies. `tests/Domain.Tests`
  (23/23) pass unchanged and `Domain.csproj` remains free of external package references.

### Consequences
- Any future aggregate root automatically participates in domain-event dispatch via
  `ApplicationDbContext` with no extra wiring.

---

## ADR-009: Hybrid Persistence — EF Core/PostgreSQL for Aggregates + Marten Event Store for Domain Events

**Date**: June 10, 2026
**Status**: Accepted

### Context
ADR-005 established Marten/PostgreSQL for event sourcing but left the relational persistence
strategy for aggregates (e.g. `Order`) and the concrete wiring (schemas, connection management,
DI) undecided.

### Decision
A single PostgreSQL 17 instance (via `docker-compose.yml` at the repo root) serves two purposes
with two schemas:
- **`public` schema**: EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL`) persists the `Order`
  aggregate relationally per ADR-007's persistence-aware shapes
  (`src/Infrastructure/Persistence/ApplicationDbContext.cs`,
  `Configurations/OrderConfiguration.cs`, `Configurations/OrderItemConfiguration.cs`,
  `Repositories/OrderRepository.cs`).
- **`event_store` schema**: Marten (`options.DatabaseSchemaName = "event_store"`) provides an
  append-only audit log of raised `IDomainEvent`s via `IEventStore`/`MartenEventStore`
  (`src/Infrastructure/EventStore/`). Each domain event is appended to a stream keyed by the
  owning aggregate's id (`MartenEventStore.ResolveStreamId`, switch over
  `Domain.Orders.Events.*`).

`ApplicationDbContext` implements `Domain.Common.IUnitOfWork`. Its `SaveChangesAsync` override
collects domain events from `IHasDomainEvents` entries (ADR-008) before
`base.SaveChangesAsync`, clears them after, then for each event calls
`IEventStore.AppendAsync` (Marten) and `IPublisher.Publish` (MediatR in-process notification) —
in that order, after the relational save succeeds.

An `IDesignTimeDbContextFactory<ApplicationDbContext>`
(`src/Infrastructure/Persistence/ApplicationDbContextFactory.cs`) with no-op `NullEventStore` /
`NullPublisher` stand-ins lets `dotnet ef migrations` run without full DI/Marten/a live
database. `dotnet-ef` is pinned via `.config/dotnet-tools.json` (local tool manifest).

### Resolved Package Versions
- `Microsoft.EntityFrameworkCore` / `.Design` / `.Relational`: 10.0.9
- `Npgsql.EntityFrameworkCore.PostgreSQL`: 10.0.2
- `Marten`: 9.7.1
- `MediatR` (Infrastructure): 14.1.0
- `Microsoft.EntityFrameworkCore.Sqlite` (Infrastructure.Tests only): 10.0.9

`Microsoft.EntityFrameworkCore.Relational` 10.0.9 was added explicitly to `Infrastructure.csproj`
to resolve an MSB3277 version-conflict warning in `src/API/API.csproj` — without it, NuGet's
"nearest wins" tie-break pulled in 10.0.4 transitively via Npgsql.

### Rationale
- One Postgres instance keeps local/dev setup simple (single `docker-compose.yml`,
  single connection string) while schema separation keeps the relational model and the
  event-sourcing audit log independent and individually evolvable.
- Dispatching domain events from `SaveChangesAsync` (rather than a separate outbox poller) keeps
  the example simple; the audit-log append and in-process publish both happen within the same
  logical unit of work as the relational save.

### Consequences
- `tests/Infrastructure.Tests` uses the EF Core SQLite in-memory provider
  (`SqliteApplicationDbContextFactory`) for DbContext/configuration/repository tests — Marten
  itself is not exercised by the automated suite (only `MartenEventStore.ResolveStreamId`, via
  `InternalsVisibleTo`).
- `docker-compose up -d` + `dotnet ef database update --project src/Infrastructure/Infrastructure.csproj
  --startup-project src/API/API.csproj` is a manual follow-up to create the actual database/schemas;
  not run as part of this session (see work log).
- Each `IEventStore.AppendAsync` call is its own Marten transaction (one per domain event) —
  acceptable for an audit log; batching is a possible future optimization.
- If a domain event's append or publish throws *after* `base.SaveChangesAsync` has committed the
  relational change, the relational write is not rolled back — this is an accepted limitation of
  the current design (true outbox/transactional-messaging is out of scope for this boilerplate).

---

## ADR-010: MediatR Pipeline Behaviors in `Application/Common/Behaviors`

**Date**: June 10, 2026
**Status**: Accepted

### Context
Cross-cutting concerns (logging, validation, unit-of-work commit) need to run around every
MediatR request handler. These behaviors depend on `MediatR`, `FluentValidation`, and
`Domain.Common.IUnitOfWork` — all available to `Application` — but the actual DI registration of
`AddDbContext`/`AddMarten`/etc. lives in `Infrastructure`.

### Decision
- `src/Application/Common/Behaviors/LoggingBehavior.cs`,
  `ValidationBehavior.cs`, `UnitOfWorkBehavior.cs` all implement
  `IPipelineBehavior<TRequest, TResponse> where TRequest : notnull`.
- `src/Application/Common/Messaging/` defines `ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>`
  marker interfaces (extending MediatR's `IRequest`/`IRequest<TResponse>`).
  `UnitOfWorkBehavior` calls `IUnitOfWork.SaveChangesAsync` only when
  `request is ICommand or ICommand<TResponse>` — queries pass through untouched.
- `ValidationBehavior` runs all registered `IValidator<TRequest>` instances and throws
  `FluentValidation.ValidationException(failures)` on failure (standard MediatR+FluentValidation
  pattern). No API-layer exception-handling middleware maps this to a `Result<T>` 400 response yet
  — tracked in `pending_decisions.md`.
- `src/Application/DependencyInjection.cs` (`AddApplication`) registers MediatR
  (`RegisterServicesFromAssembly`), the three behaviors in order
  Logging → Validation → UnitOfWork (outermost first), `AddValidatorsFromAssembly`, and
  `AddAutoMapper(_ => { }, assembly)` — note AutoMapper 16.1.1's `IServiceCollection` extension
  requires the `Action<IMapperConfigurationExpression>` overload; `AddAutoMapper(assembly)` alone
  does not compile.
- `src/Infrastructure/DependencyInjection.cs` (`AddInfrastructure`) only wires Infrastructure's
  own services (DbContext, repositories, Marten/event store) — it does not touch MediatR or the
  pipeline.

### Rationale
- Keeps cross-cutting request/response concerns colocated with the CQRS abstractions they act on
  (`Application` layer), per Clean Architecture convention, while Infrastructure remains focused
  on persistence/external-system wiring.

### Consequences
- `src/API/Program.cs` must call both `AddApplication()` and `AddInfrastructure(configuration)`
  for the application to function.
- `tests/Application.Tests/Common/Behaviors/` contains Moq-based unit tests for all three
  behaviors using `public sealed record` test request/command/query types (Moq + Castle
  DynamicProxy cannot proxy interfaces whose generic arguments are non-public types).

---

## Note: `LoggingExtensions` Duplication (Application / Infrastructure)

`src/Application/Common/Logging/LoggingExtensions.cs` and
`src/Infrastructure/Common/Logging/LoggingExtensions.cs` are intentionally near-identical
(~30 lines each, namespaces `Application.Common.Logging` / `Infrastructure.Common.Logging`)
implementing the project's `[LogLevel][Layer][Incremental]` structured Log ID convention
(Application = 2xxx, Infrastructure = 4xxx). Infrastructure cannot reference Application (wrong
dependency direction), so the helper is duplicated rather than shared. This is a tracked, accepted
minor duplication — a candidate for extraction into a future shared "SeedWork"/kernel package if a
third layer ever needs the same convention.

---

## ADR-011: Named Per-Event Logging Extension Methods

**Date**: June 10, 2026
**Status**: Accepted

### Context
Code review of `LoggingBehavior` flagged direct calls to the generic
`LogInformationWithId`/`LogErrorWithId` helpers with inline log IDs and message templates
(e.g. `_logger.LogInformationWithId(2201, "Handling {RequestName}", requestName)`). This
scatters log IDs and message templates across call sites, making them hard to audit/grep and
easy to duplicate or drift.

### Decision
Generic `LogXWithId` helpers in `LoggingExtensions.cs` remain (they implement the structured
log-id prefixing convention), but call sites (behaviors, handlers, services) must not invoke
them directly. Instead, each distinct log event gets its own named extension method in the
layer's `LoggingExtensions.cs`, e.g.:
```csharp
public static void LogHandlingRequest(this ILogger logger, string requestName)
    => logger.LogInformationWithId(2201, "Handling {RequestName}", requestName);
```
`LoggingBehavior` now calls `_logger.LogHandlingRequest(requestName)`,
`_logger.LogHandledRequest(requestName)`, `_logger.LogUnhandledRequestException(ex,
requestName)`; `MartenEventStore` calls `_logger.LogAppendedDomainEvent(eventType, streamId)`.

### Rationale
- Centralizes every log ID + message template in one file per layer — easy to audit for ID
  collisions and to see the full catalog of log events a layer can emit.
- Keeps call sites self-documenting and free of magic numbers/format strings.

### Consequences
- Adding a new log event means adding a small named method to `LoggingExtensions.cs` first.
  This is a deliberate, low-cost extra step in exchange for centralization.

---

## ADR-012: DI Registration via `Extensions/ServiceExtension.cs` Per Layer

**Date**: June 10, 2026
**Status**: Accepted

### Context
`src/Application/DependencyInjection.cs` and `src/Infrastructure/DependencyInjection.cs`
(static classes named `DependencyInjection`, each exposing one `Add<Layer>` extension method)
worked but didn't follow a documented file/folder convention, and the class name shadowed the
common `Microsoft.Extensions.DependencyInjection` namespace name.

### Decision
Each layer's DI registration extension method moves to
`src/<Layer>/Extensions/ServiceExtension.cs`, namespace `<Layer>.Extensions`, static class
`ServiceExtension`. The method itself is unchanged (`AddApplication`, `AddInfrastructure`).
`src/API/Program.cs` and tests now `using Application.Extensions;` /
`using Infrastructure.Extensions;`.

### Rationale
- `Extensions/ServiceExtension.cs` is an explicit, predictable location/name for "how does this
  layer wire itself into DI" — scales cleanly if a layer ever needs more than one extension
  method (still one class, multiple methods, or split by concern under the same folder).
- Avoids a type named `DependencyInjection` colliding (in intent, if not in compiled IL) with
  the `Microsoft.Extensions.DependencyInjection` namespace it sits alongside.

### Consequences
- Future layers (e.g. a `Persistence`-only or `Messaging`-only sub-module) follow the same
  `Extensions/ServiceExtension.cs` + `Add<Thing>` pattern.

---

## ADR-013: Local PostgreSQL Secrets via `.env` (Docker Compose)

**Date**: June 10, 2026
**Status**: Accepted

### Context
The original `docker-compose.yml` hardcoded `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD`
as committed plaintext (labeled "development defaults only"), and
`ApplicationDbContextFactory` carried a matching hardcoded `DefaultConnectionString` fallback
containing the same credentials. Code review flagged both as violating the "no hardcoded
secrets, including dev defaults" rule in the Review Checklist.

### Decision
- `docker-compose.yml` no longer hardcodes Postgres credentials; it references
  `${POSTGRES_DB}`, `${POSTGRES_USER}`, `${POSTGRES_PASSWORD}`, `${POSTGRES_PORT:-5432}`, which
  docker-compose resolves from a gitignored `.env` file at the repo root.
- A committed `.env.example` documents the expected variables (with the same values previously
  hardcoded, now as a *template* a developer copies to `.env`).
- `.gitignore` now excludes `.env` alongside `appsettings.Development.json`.
- `ApplicationDbContextFactory.CreateDbContext` no longer has a `DefaultConnectionString`
  fallback. If `ConnectionStrings:Database` isn't found via `appsettings.json` /
  `appsettings.Development.json` / environment variables (`AddEnvironmentVariables()` added),
  it throws `InvalidOperationException` with setup instructions referencing `.env.example` and
  `appsettings.Development.json`.
- `tests/Infrastructure.Tests/appsettings.json` (committed) holds a non-functional placeholder
  connection string (`Host=localhost;...;Username=test;Password=test`) used only so
  `AddInfrastructure`'s configuration check passes during DI-registration tests — no real
  connection is opened, so this is not a secret and needs no `appsettings.Development.json`
  counterpart.

### Rationale
- Removes the only two places a real-looking Postgres credential pair was committed to source
  control, while keeping local setup a one-line `cp .env.example .env`.
- A hard failure with actionable guidance is preferable to silently falling back to a
  credential pair that may or may not match the developer's actual local database.

### Consequences
- First-time setup requires `cp .env.example .env` (and creating
  `src/API/appsettings.Development.json` per ADR-009) before `docker-compose up -d` /
  `dotnet ef database update` will work — documented in the work log as a manual step.

---

## ADR-014: Order CQRS Use-Cases — One File Per Use Case, `NotFoundException`, Independent DTOs

**Date**: June 11, 2026
**Status**: Accepted

### Context
`src/Application/Orders/` needed concrete CQRS commands/queries for the `Order` aggregate
(CreateOrder, AddOrderItem, PlaceOrder, ShipOrder, CancelOrder, GetOrderById), building on the
pipeline/marker interfaces from ADR-010. Four related questions arose: how to organize each
use case's files, how to represent API-facing data, how to enum-mirror `Domain.Orders.OrderStatus`,
and how command handlers vs. the query handler should signal "order not found".

### Decision
1. **One file per CQRS use case** (explicit, user-directed exception to "one public class per
   file"): each use case lives in a single `<UseCase><Command|Query>.cs` file under
   `src/Application/<BoundedContext>/Commands|Queries/`, containing exactly three public types —
   `<UseCase>Command`/`<UseCase>Query` (the MediatR request record), `<UseCase>Validator`
   (`AbstractValidator<...>`), and `<UseCase>Handler` (`IRequestHandler<...>`). Documented in
   `user_preference.md` under "CQRS Use-Case Files". DTOs and mapping profiles remain one public
   type per file.
2. **`Application.Common.Exceptions.NotFoundException`** (`src/Application/Common/Exceptions/NotFoundException.cs`):
   a simple `Exception` subclass taking `(string entityName, object key)`, formatted as
   `Entity "{entityName}" ({key}) was not found.`. Thrown by command handlers
   (`AddOrderItemCommand`, `PlaceOrderCommand`, `ShipOrderCommand`, `CancelOrderCommand`) when
   `IOrderRepository.GetByIdAsync` returns `null`.
3. **`GetOrderByIdQuery` returns `OrderDto?`** (null on not-found) — the read path does not
   throw; mapping "no data" to a 404 vs. an empty/`Result<T>` response is left to the future
   API layer.
4. **`src/Application/Orders/Dtos/`** (`AddressDto`, `OrderItemDto`, `OrderDto`, `OrderStatusDto`)
   are plain immutable records, fully independent of `Domain.Orders` types — `Money` is
   flattened to `(decimal Amount-equivalent, string Currency)` pairs, strongly-typed IDs
   (`OrderId`/`CustomerId`/`ProductId`/`OrderItemId`) become `Guid`, and `OrderStatusDto` is a
   separate enum mirroring `Domain.Orders.OrderStatus` by name/value (`Draft=0, Placed=1,
   Shipped=2, Cancelled=3`).
5. **`src/Application/Orders/Mappings/OrderMappingProfile.cs`** (`AutoMapper.Profile`,
   auto-registered via `AddAutoMapper(_ => {}, assembly)`) maps `Order -> OrderDto`,
   `OrderItem -> OrderItemDto`, `Address -> AddressDto`, `OrderStatus -> OrderStatusDto`, and
   `ConvertUsing(id => id.Value)` for each strongly-typed ID -> `Guid`.

### Rationale
- One-file-per-use-case keeps each vertical slice (request, validation, handling) self-contained
  and easy to navigate, at the cost of slightly larger files — acceptable given each use case is
  small (a handful of lines per type).
- A dedicated `NotFoundException` keeps Domain free of Application/HTTP concerns while giving
  command handlers a single, greppable signal; mapping it to a `Result<T>` 404 is an API-layer
  concern (tracked in `pending_decisions.md`).
- Returning `null` (not throwing) for `GetOrderByIdQuery` follows the common CQRS convention that
  reads are side-effect-free and "not found" is normal, expected output for a query, not an
  exceptional condition.
- Independent DTOs/`OrderStatusDto` prevent leaking Domain types (and any future Domain changes)
  across the Application/API boundary.

### Implementation Detail: AutoMapper + Positional Records
`OrderDto` and `OrderItemDto` are declared as records with `init`-only properties (and an
implicit parameterless constructor) rather than positional records (`record Foo(...)`).
AutoMapper 16.1.1's constructor-mapping for positional records requires either exact
constructor-parameter/source-member name matches or explicit `.ForCtorParam(...)` for every
constructor parameter; combining `.ForMember(...)` (used to flatten `Money` into
amount/currency pairs) with a positional record's primary constructor produced
`AutoMapperConfigurationException: ... No available constructor` and, at runtime,
`ArgumentException: ... needs to have a constructor with 0 args or only optional args`.
Switching to `init`-property records with a parameterless constructor lets `.ForMember(...)`
map every property (including the flattened `Money` ones) via reflection, with no
`.ForCtorParam` needed. The `required` modifier on each property is a compile-time-only
constraint and does not affect AutoMapper's reflection-based construction.

### Consequences
- Future bounded contexts follow the same `Commands/`, `Queries/`, `Dtos/`, `Mappings/` layout
  and one-file-per-use-case convention.
- Any future AutoMapper-mapped DTO that flattens value objects (`Money`, etc.) via `.ForMember`
  should use `init`-property records (not positional records) to avoid the constructor-mapping
  pitfall described above.
- `tests/Application.Tests/Orders/` (28 new tests: 10 command handler + 10 command validator +
  2 query handler + 1 query validator + 2 mapping profile tests, plus `CreateOrderHandlerTests`/
  `CreateOrderValidatorTests` already counted) brings `Application.Tests` to 38/38.

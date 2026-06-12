# ADR-009: Hybrid Persistence — EF Core/PostgreSQL for Aggregates + Marten Event Store for Domain Events

**Date**: June 10, 2026
**Status**: Accepted

## Context
ADR-005 established Marten/PostgreSQL for event sourcing but left the relational persistence
strategy for aggregates (e.g. `Order`) and the concrete wiring (schemas, connection management,
DI) undecided.

## Decision
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

## Resolved Package Versions
- `Microsoft.EntityFrameworkCore` / `.Design` / `.Relational`: 10.0.9
- `Npgsql.EntityFrameworkCore.PostgreSQL`: 10.0.2
- `Marten`: 9.7.1
- `MediatR` (Infrastructure): 14.1.0
- `Microsoft.EntityFrameworkCore.Sqlite` (Infrastructure.Tests only): 10.0.9

`Microsoft.EntityFrameworkCore.Relational` 10.0.9 was added explicitly to `Infrastructure.csproj`
to resolve an MSB3277 version-conflict warning in `src/API/API.csproj` — without it, NuGet's
"nearest wins" tie-break pulled in 10.0.4 transitively via Npgsql.

## Rationale
- One Postgres instance keeps local/dev setup simple (single `docker-compose.yml`,
  single connection string) while schema separation keeps the relational model and the
  event-sourcing audit log independent and individually evolvable.
- Dispatching domain events from `SaveChangesAsync` (rather than a separate outbox poller) keeps
  the example simple; the audit-log append and in-process publish both happen within the same
  logical unit of work as the relational save.

## Consequences
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

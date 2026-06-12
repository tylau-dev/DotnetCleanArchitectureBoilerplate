# Architectural Decisions — Index

Full ADR text lives in `adr/ADR-NNN-*.md`. This index is part of the **hot tier** (read every
session, see `agent.md`) — it's short by design. Open an individual ADR file only when your
current task touches the area it covers.

| ADR | Title | Summary | Areas touched |
|-----|-------|---------|----------------|
| [ADR-001](adr/ADR-001-clean-architecture-ddd-cqrs.md) | Clean Architecture + DDD + CQRS Pattern | Domain/Application/Infrastructure/API layering with DDD aggregates and CQRS handlers | all layers |
| [ADR-002](adr/ADR-002-result-wrapper.md) | Standardized `Result<T>` API Response Wrapper | All API responses use `Result<T>` (Success, Data, Errors, StatusCode) | API |
| [ADR-003](adr/ADR-003-dotnet10-target-framework.md) | .NET 10 as Target Framework | Target framework is .NET 10 / C# 14 | build |
| [ADR-004](adr/ADR-004-separate-test-projects.md) | Separate Unit and Integration Test Projects | `Domain.Tests` (unit) and `Application.Tests` (integration) are separate projects | tests |
| [ADR-005](adr/ADR-005-marten-event-sourcing.md) | Marten for Event Sourcing with Docker Support | Marten + PostgreSQL for event sourcing, Docker Compose for local dev | infrastructure, event sourcing |
| [ADR-006](adr/ADR-006-dev-practices-doc.md) | Development Practices Documentation | Practices documented in `user_preference.md` | docs |
| [ADR-007](adr/ADR-007-persistence-aware-domain-entities.md) | Persistence-Aware Domain Entities — No Separate Persistence Model | Domain entities shaped for direct EF Core mapping; no separate persistence model/mapper | domain, infrastructure/persistence |
| [ADR-008](adr/ADR-008-ihasdomainevents-marker-interface.md) | `IHasDomainEvents` Marker Interface on `AggregateRoot<TId>` | Non-generic marker interface lets `ApplicationDbContext` collect domain events across aggregate types | domain, infrastructure/persistence |
| [ADR-009](adr/ADR-009-hybrid-persistence.md) | Hybrid Persistence — EF Core/PostgreSQL + Marten Event Store | Single Postgres instance, `public` schema (EF Core aggregates) + `event_store` schema (Marten audit log); resolved package versions | infrastructure/persistence, event sourcing |
| [ADR-010](adr/ADR-010-mediatr-pipeline-behaviors.md) | MediatR Pipeline Behaviors in `Application/Common/Behaviors` | `LoggingBehavior`, `ValidationBehavior`, `UnitOfWorkBehavior`; `ICommand`/`ICommand<T>`/`IQuery<T>` marker interfaces; `AddApplication` registration order | application, CQRS pipeline |
| [NOTE](adr/NOTE-loggingextensions-duplication.md) | `LoggingExtensions` Duplication (Application / Infrastructure) | Near-identical `LoggingExtensions.cs` in both layers is intentional (no cross-layer dependency) | application, infrastructure, logging |
| [ADR-011](adr/ADR-011-named-logging-extension-methods.md) | Named Per-Event Logging Extension Methods | Every log event gets its own named extension method wrapping `LogXWithId`; no inline log IDs at call sites | logging, all layers |
| [ADR-012](adr/ADR-012-service-extension-di-registration.md) | DI Registration via `Extensions/ServiceExtension.cs` Per Layer | Each layer's `Add<Layer>` DI extension lives in `src/<Layer>/Extensions/ServiceExtension.cs` | DI, all layers |
| [ADR-013](adr/ADR-013-postgres-secrets-env.md) | Local PostgreSQL Secrets via `.env` (Docker Compose) | `docker-compose.yml` reads credentials from gitignored `.env` (`.env.example` template); no hardcoded fallback connection string | infrastructure, configuration/secrets |
| [ADR-014](adr/ADR-014-order-cqrs-use-cases.md) | Order CQRS Use-Cases — One File Per Use Case, `NotFoundException`, Independent DTOs | One file per CQRS use case (Command/Query + Validator + Handler); `NotFoundException`; independent DTOs + AutoMapper profile; `init`-property records for AutoMapper `.ForMember` | application/Orders, CQRS, DTOs |

## Conventions referenced elsewhere
The "CQRS Use-Case Files" convention (ADR-014) and the "DI Registration" convention (ADR-012)
are also documented in `user_preference.md` for day-to-day reference — no need to open the ADR
files just to recall those conventions.

## Adding a new ADR
1. Create `adr/ADR-NNN-short-slug.md` using the same `# ADR-NNN: Title` / Context / Decision /
   Rationale / Consequences structure as existing files.
2. Add one row to the table above.

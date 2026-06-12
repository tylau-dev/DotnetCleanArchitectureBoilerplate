# ADR-010: MediatR Pipeline Behaviors in `Application/Common/Behaviors`

**Date**: June 10, 2026
**Status**: Accepted

## Context
Cross-cutting concerns (logging, validation, unit-of-work commit) need to run around every
MediatR request handler. These behaviors depend on `MediatR`, `FluentValidation`, and
`Domain.Common.IUnitOfWork` — all available to `Application` — but the actual DI registration of
`AddDbContext`/`AddMarten`/etc. lives in `Infrastructure`.

## Decision
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

## Rationale
- Keeps cross-cutting request/response concerns colocated with the CQRS abstractions they act on
  (`Application` layer), per Clean Architecture convention, while Infrastructure remains focused
  on persistence/external-system wiring.

## Consequences
- `src/API/Program.cs` must call both `AddApplication()` and `AddInfrastructure(configuration)`
  for the application to function.
- `tests/Application.Tests/Common/Behaviors/` contains Moq-based unit tests for all three
  behaviors using `public sealed record` test request/command/query types (Moq + Castle
  DynamicProxy cannot proxy interfaces whose generic arguments are non-public types).

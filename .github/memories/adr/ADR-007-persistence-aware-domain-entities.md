# ADR-007: Persistence-Aware Domain Entities — No Separate Persistence Model

**Date**: June 10, 2026
**Status**: Accepted

## Context
The Domain layer must remain free of external dependencies (no EF Core, MediatR, etc.) per
Clean Architecture rules, while Infrastructure will later need to map these types directly to
an EF Core `DbContext`. Two approaches were considered:
1. Pure domain models + a separate set of EF Core persistence/DTO models with a mapping layer.
2. A single set of domain types, shaped so EF Core's Fluent API can map them directly, with no
   mapper layer.

## Decision
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

## Rationale
- Avoids duplicating the model and writing/maintaining a mapping layer between domain and
  persistence representations — a common source of drift and boilerplate.
- Keeps `Domain.csproj` free of any package references beyond what `Directory.Build.props`
  injects (verified via `dotnet list package`).
- EF Core 8+'s constructor binding and private-member access make it practical to map
  encapsulated, behavior-rich entities directly without exposing public setters.

## Consequences
- Infrastructure's future EF Core configuration must use `IEntityTypeConfiguration<T>` with
  Fluent API (backing-field access for `_items`, owned-type configuration for `Money`/`Address`,
  value converters for the strongly-typed ID structs) — no Domain-side changes required.
- Domain entities carry a couple of EF-oriented constructs (private parameterless ctors,
  `= null!` defaults for owned-type properties) that exist solely to support future
  materialization; this is an accepted, documented trade-off in exchange for avoiding a
  parallel persistence model.
- If a future bounded context genuinely needs a divergent persistence shape, that context can
  introduce its own mapper without affecting this convention elsewhere.

# ADR-008: `IHasDomainEvents` Marker Interface on `AggregateRoot<TId>`

**Date**: June 10, 2026
**Status**: Accepted

## Context
Infrastructure's `ApplicationDbContext.SaveChangesAsync` override needs to collect and dispatch
domain events from all tracked aggregates before/after persistence, without depending on the
generic `TId` parameter of `AggregateRoot<TId>` (EF Core's `ChangeTracker.Entries<T>()` requires a
single, non-generic type to query against).

## Decision
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

## Rationale
- Lets `ApplicationDbContext.SaveChangesAsync` use
  `ChangeTracker.Entries<IHasDomainEvents>().SelectMany(e => e.Entity.DomainEvents)` to collect
  events across all aggregate types in a single query.
- Purely additive — no existing members changed, no new dependencies. `tests/Domain.Tests`
  (23/23) pass unchanged and `Domain.csproj` remains free of external package references.

## Consequences
- Any future aggregate root automatically participates in domain-event dispatch via
  `ApplicationDbContext` with no extra wiring.

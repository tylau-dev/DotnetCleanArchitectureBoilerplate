# Work Log: 2026-06-10 — feature/design_domain_layer

## Session Goal
Design and implement `src/Domain` for the example "Order Management" bounded context using
DDD building blocks (aggregates, entities, value objects, domain events, repository
interfaces), with entities shaped for direct EF Core mapping and zero external dependencies.

## Completed
- Removed placeholder `src/Domain/Class1.cs` and `tests/Domain.Tests/UnitTest1.cs`.
- Added `src/Domain/Common/` SeedWork:
  - `IDomainEvent`, `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IUnitOfWork`,
    `DomainException`, `InvalidOrderStateException`.
- Added `src/Domain/Orders/ValueObjects/`:
  - `OrderId`, `OrderItemId`, `CustomerId`, `ProductId` (`readonly record struct` over `Guid`
    with `New()` factories), `Money` (Add/Subtract/Multiply, currency-mismatch guard), `Address`.
- Added `src/Domain/Orders/Events/`:
  - `OrderItemAddedDomainEvent`, `OrderPlacedDomainEvent`, `OrderShippedDomainEvent`,
    `OrderCancelledDomainEvent` (all `record` implementing `IDomainEvent`).
- Added `src/Domain/Orders/`:
  - `OrderStatus` enum (`Draft`, `Placed`, `Shipped`, `Cancelled`).
  - `OrderItem` entity (`Entity<OrderItemId>`), internal constructor used by `Order.AddItem`.
  - `Order` aggregate root (`AggregateRoot<OrderId>`) with `Create`, `AddItem`, `Place`, `Ship`,
    `Cancel`, computed `TotalAmount`.
  - `IOrderRepository` (`GetByIdAsync`, `AddAsync`, `ExistsAsync`).
- Added `tests/Domain.Tests/` (xUnit, AAA, `Should_X_When_Y` naming):
  - `Common/ValueObjectTests.cs`, `Orders/ValueObjects/MoneyTests.cs`,
    `Orders/ValueObjects/AddressTests.cs`, `Orders/OrderTests.cs` (23 tests total).
  - Added `<ProjectReference>` from `Domain.Tests.csproj` to `Domain.csproj` (was missing).
- Updated `.github/project.md` — Example Domain set to "Order Management".
- Added ADR-007 to `.github/memories/architectural_decisions.md` (persistence-aware domain
  entities, no separate persistence model).

## Verification
- `dotnet build` — succeeds, 0 errors, 0 new warnings (4 pre-existing CA5394 warnings in
  `src/API/Program.cs`, unrelated to this change).
- `dotnet test` — all pass (Domain.Tests: 23/23, Application.Tests: 1/1).
- `dotnet list src/Domain/Domain.csproj package` — only `AsyncFixer` and
  `Microsoft.CodeAnalysis.NetAnalyzers` (both injected by `Directory.Build.props`); no new
  package references added to Domain.
- No project references added from Domain to any other layer.

## Decisions Made
- `Money.Multiply(int quantity)` added (not explicitly in spec) to compute `OrderItem.Subtotal`
  cleanly; keeps `Order.TotalAmount` as a simple aggregation of subtotals.
- `Order.TotalAmount` for an order with no items returns `Money.Zero("USD")` — a
  `DefaultCurrency` constant on `Order`, documented inline as the assumed currency before any
  item establishes the order's actual currency.
- `InvalidOrderStateException` placed in `Domain/Common/` per spec, even though it is
  Order-specific, alongside the abstract `DomainException` base.

## Next Steps (Not Started)
- Application layer: CQRS commands/queries (e.g., CreateOrder, AddOrderItem, PlaceOrder,
  ShipOrder, CancelOrder) and DTOs over the `Order` aggregate.
- Infrastructure layer: EF Core `DbContext`, `IEntityTypeConfiguration<Order>` /
  `IEntityTypeConfiguration<OrderItem>` (Fluent API, backing-field access for `_items`, owned
  types for `Money`/`Address`, value converters for the strongly-typed ID structs),
  `IOrderRepository` and `IUnitOfWork` implementations, MediatR domain-event dispatch.
- API layer: Minimal API endpoints mapping to the above commands/queries via `Result<T>`.

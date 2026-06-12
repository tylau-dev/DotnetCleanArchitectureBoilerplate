# ADR-014: Order CQRS Use-Cases — One File Per Use Case, `NotFoundException`, Independent DTOs

**Date**: June 11, 2026
**Status**: Accepted

## Context
`src/Application/Orders/` needed concrete CQRS commands/queries for the `Order` aggregate
(CreateOrder, AddOrderItem, PlaceOrder, ShipOrder, CancelOrder, GetOrderById), building on the
pipeline/marker interfaces from ADR-010. Four related questions arose: how to organize each
use case's files, how to represent API-facing data, how to enum-mirror `Domain.Orders.OrderStatus`,
and how command handlers vs. the query handler should signal "order not found".

## Decision
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

## Rationale
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

## Implementation Detail: AutoMapper + Positional Records
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

## Consequences
- Future bounded contexts follow the same `Commands/`, `Queries/`, `Dtos/`, `Mappings/` layout
  and one-file-per-use-case convention.
- Any future AutoMapper-mapped DTO that flattens value objects (`Money`, etc.) via `.ForMember`
  should use `init`-property records (not positional records) to avoid the constructor-mapping
  pitfall described above.
- `tests/Application.Tests/Orders/` (28 new tests: 10 command handler + 10 command validator +
  2 query handler + 1 query validator + 2 mapping profile tests, plus `CreateOrderHandlerTests`/
  `CreateOrderValidatorTests` already counted) brings `Application.Tests` to 38/38.

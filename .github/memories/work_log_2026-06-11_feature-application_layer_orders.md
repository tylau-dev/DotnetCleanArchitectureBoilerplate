# Work Log: 2026-06-11 — feature/application_layer_orders

## Session Goal
Build out `src/Application/Orders/` — the remaining CQRS commands/queries, handlers, DTOs,
and AutoMapper profile for the `Order` aggregate (CreateOrder, AddOrderItem, PlaceOrder,
ShipOrder, CancelOrder, GetOrderById), reusing the pipeline behaviors and CQRS marker
interfaces built in the prior session.

## Completed

### Convention
- Documented a new "CQRS Use-Case Files" convention in `.github/user_preference.md`
  (explicit exception to "one public class per file"): each use case lives in a single
  `<UseCase><Command|Query>.cs` file containing the request record, its
  `<UseCase>Validator`, and its `<UseCase>Handler`.

### Common
- Added `src/Application/Common/Exceptions/NotFoundException.cs` —
  `Exception` subclass `(string entityName, object key)` ->
  `Entity "{entityName}" ({key}) was not found.`, thrown by command handlers when
  `IOrderRepository.GetByIdAsync` returns `null`.

### DTOs (`src/Application/Orders/Dtos/`)
- `AddressDto`, `OrderItemDto`, `OrderDto` — `init`-property records (not positional records)
  with `required` modifiers, fully independent of `Domain.Orders` types.
- `OrderStatusDto` — enum mirroring `Domain.Orders.OrderStatus` (`Draft=0, Placed=1,
  Shipped=2, Cancelled=3`).

### Mapping (`src/Application/Orders/Mappings/OrderMappingProfile.cs`)
- `AutoMapper.Profile` mapping `OrderId/CustomerId/ProductId/OrderItemId -> Guid`
  (`ConvertUsing(id => id.Value)`), `OrderStatus -> OrderStatusDto`, `Address -> AddressDto`,
  `OrderItem -> OrderItemDto` (flattens `Money` via `.ForMember`), `Order -> OrderDto`
  (flattens `TotalAmount` via `.ForMember`).

### Commands (`src/Application/Orders/Commands/`)
- `CreateOrderCommand.cs` — `ICommand<Guid>`; builds `Order.Create(...)`, calls
  `orderRepository.AddAsync(order, ct)`, returns `order.Id.Value`.
- `AddOrderItemCommand.cs` — `ICommand`; fetch-or-`NotFoundException`, calls
  `order.AddItem(...)`.
- `PlaceOrderCommand.cs` / `ShipOrderCommand.cs` / `CancelOrderCommand.cs` — `ICommand`;
  fetch-or-`NotFoundException`, call `order.Place()` / `order.Ship()` / `order.Cancel()`.
- None of the handlers call `IUnitOfWork.SaveChangesAsync` — `UnitOfWorkBehavior` (ADR-010)
  does this automatically for any `ICommand`/`ICommand<T>`.

### Query (`src/Application/Orders/Queries/GetOrderByIdQuery.cs`)
- `GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>` — `GetOrderByIdHandler` returns `null`
  when the order doesn't exist, otherwise `mapper.Map<OrderDto>(order)`. No exception on the
  read path; not-found-vs-data is left to the future API layer.

### Tests (`tests/Application.Tests/Orders/`)
- 5 command handler test files + 5 command validator test files (happy path +
  `NotFoundException` for AddOrderItem/Place/Ship/Cancel).
- `GetOrderByIdHandlerTests` (mapped result + null-on-not-found) and
  `GetOrderByIdValidatorTests`.
- `OrderMappingProfileTests` (`AssertConfigurationIsValid()` + round-trip `Order -> OrderDto`
  mapping).
- 28 new tests; `Application.Tests` total 38/38.

## Issue Encountered & Fixed

### AutoMapper + positional records + `.ForMember`
`OrderDto`/`OrderItemDto` were initially declared as positional records
(`record Foo(...)`). Combining `.ForMember(...)` (to flatten `Money` into amount/currency
pairs) with a positional record's primary constructor caused:
- `AutoMapperConfigurationException`: "Unmapped members were found... No available
  constructor" for `OrderItem -> OrderItemDto` and `Order -> OrderDto`.
- At runtime: `ArgumentException: ... needs to have a constructor with 0 args or only
  optional args`.

**Fix**: rewrote `AddressDto`, `OrderItemDto`, `OrderDto` as records with `init`-only
`required` properties and an implicit parameterless constructor, instead of positional
records. AutoMapper then maps every property (including the `.ForMember`-flattened ones) via
reflection with no `.ForCtorParam` needed. The `required` modifier is compile-time-only and
doesn't affect AutoMapper's reflection-based construction. Also fixed an earlier, separate
build error: AutoMapper 16.1.1's `MapperConfiguration` constructor requires
`(Action<IMapperConfigurationExpression>, ILoggerFactory)` — added
`Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance` as the second argument
in `OrderMappingProfileTests.cs` and `GetOrderByIdHandlerTests.cs`.

## Verification
- `dotnet build` (whole solution): **Build succeeded, 0 Errors**, 4 pre-existing CA5394
  ("insecure Random") warnings in `src/API/Program.cs` unchanged.
- `dotnet test` (whole solution): `Domain.Tests` 23/23, `Application.Tests` 38/38,
  `Infrastructure.Tests` 11/11 — all passing.

## Memory Updates
- `.github/user_preference.md`: added "CQRS Use-Case Files" convention.
- `.github/memories/architectural_decisions.md`: added ADR-014 (one-file-per-use-case,
  `NotFoundException`, independent DTOs, `OrderStatusDto`, `GetOrderByIdQuery` returning
  `OrderDto?`, AutoMapper init-property-record fix).
- `.github/memories/pending_decisions.md`: added "API-Layer Mapping of `NotFoundException` to
  `Result<T>` 404" alongside the existing `ValidationException` -> 400 entry.
- `.github/memories/current_state.md`: added Session 7 summary; updated "Missing Components"
  to drop `src/Application/Orders/` and keep `src/API/` endpoints + exception-handling
  middleware as remaining work.

## Out of Scope (untouched, per instructions)
- `src/API/` — Minimal API endpoints for the new Order commands/queries (next session).
- API-layer exception-handling middleware for `ValidationException`/`NotFoundException`.
- `docker-compose up -d` + `dotnet ef database update`.

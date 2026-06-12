# Current Project State

_Hot tier — reflects ONLY the current state. Overwritten each session, never appended to.
Full session-by-session history lives in `work_log_*.md` (cold tier, not read by default)._

**Last updated**: Session 7 (June 11, 2026) — branch `feature/api_layer`

## Last Completed Work (Session 7 — Application Layer: Order CQRS Use Cases)
- `src/Application/Common/Exceptions/NotFoundException.cs` (new)
- `src/Application/Orders/Dtos/`: `AddressDto`, `OrderItemDto`, `OrderDto`, `OrderStatusDto`
- `src/Application/Orders/Mappings/OrderMappingProfile.cs` (AutoMapper profile, see ADR-014)
- `src/Application/Orders/Commands/`: CreateOrder, AddOrderItem, PlaceOrder, ShipOrder,
  CancelOrder — each file = Command + Validator + Handler (ADR-014 "one file per use case")
- `src/Application/Orders/Queries/GetOrderByIdQuery.cs` (returns `OrderDto?`, null on not-found)
- `tests/Application.Tests/Orders/` — 28 new tests (handlers, validators, mapping profile)
- ADR-014 added (see `architectural_decisions.md`)
- `dotnet build`: 0 errors, 4 pre-existing CA5394 warnings unchanged.
  `dotnet test`: Domain.Tests 23/23, Application.Tests 38/38, Infrastructure.Tests 11/11.

## Project Structure (Current)
```
src/
├── API/           (Program.cs wires AddApplication + AddInfrastructure)
├── Application/
│   ├── Common/{Behaviors,Logging,Messaging,Exceptions}
│   ├── Orders/{Commands,Queries,Dtos,Mappings}
│   └── Extensions/ServiceExtension.cs (AddApplication, ADR-012)
├── Domain/        (Order Management aggregate)
└── Infrastructure/
    ├── Common/Logging/
    ├── EventStore/ (Marten)
    ├── Extensions/ServiceExtension.cs (AddInfrastructure, ADR-012)
    └── Persistence/ (EF Core: Configurations, Migrations, Repositories)
tests/
├── Application.Tests/  (38 tests)
├── Domain.Tests/        (23 tests)
└── Infrastructure.Tests/ (11 tests)
```

## Next Up
- `src/API/` — Minimal API endpoints mapping to the Order commands/queries via `Result<T>`
- API-layer exception handling: `ValidationException` → `Result<T>` 400 and
  `NotFoundException` → `Result<T>` 404 (see `pending_decisions.md`)
- `docker-compose up -d` + `dotnet ef database update` (manual, not yet run)
- ARCHITECTURE.md, CONTRIBUTING.md

## Ready to Proceed
Yes — Order CQRS use cases (commands, query, DTOs, mapping profile, validators, tests) are
complete and verified.

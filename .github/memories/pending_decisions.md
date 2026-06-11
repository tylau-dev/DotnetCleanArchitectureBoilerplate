# Pending Decisions

## Summary
Pending items below; otherwise all major architectural decisions have been confirmed.

---

## Open: IDE0290 "Use primary constructor" suggestion across behavior/service classes

**Raised**: June 10, 2026 (IDE diagnostics on `UnitOfWorkBehavior.cs`, suggestion severity)

The IDE suggests IDE0290 "Use primary constructor" for most behavior/service classes that
have a single-purpose constructor (e.g. `UnitOfWorkBehavior`, `ValidationBehavior`,
`LoggingBehavior`, `MartenEventStore`, `ApplicationDbContext`, `OrderRepository`). This is a
suggestion-severity, non-blocking diagnostic. Whether to adopt primary constructors
repo-wide (and how that interacts with XML doc comments on constructor parameters) is a
separate, unresolved style decision — not addressed in this session.

---

## Open: CA1062 "Validate parameter is non-null" on pipeline behavior `next` delegate

**Raised**: June 10, 2026 (IDE diagnostics on `UnitOfWorkBehavior.cs`, suggestion severity)

CA1062 suggests validating that `next` (the `RequestHandlerDelegate<TResponse>` parameter of
`Handle(..., RequestHandlerDelegate<TResponse> next, ...)`) is non-null before use, in
`UnitOfWorkBehavior`, `ValidationBehavior`, and `LoggingBehavior`. MediatR guarantees `next`
is non-null, so adding a defensive null-check would contradict the "don't validate what can't
happen" principle. This is a separate, unresolved decision on whether/how to suppress this
diagnostic for these pipeline behaviors — not addressed in this session.

---

## Open: API-Layer Mapping of `FluentValidation.ValidationException` to `Result<T>`

**Raised**: June 10, 2026 (during Application/Infrastructure layer session, see ADR-010)

`Application.Common.Behaviors.ValidationBehavior` throws
`FluentValidation.ValidationException` when registered validators fail. Per ADR-002,
API responses should use the `Result<T>` wrapper with appropriate `StatusCode` (400 for
validation errors). No API-layer exception-handling middleware (e.g. an `IExceptionHandler`)
currently catches `ValidationException` and maps it to a `Result<T>` 400 response — this is
expected to be addressed when `src/API` endpoints and CQRS handlers are added.

---

## Open: API-Layer Mapping of `Application.Common.Exceptions.NotFoundException` to `Result<T>` 404

**Raised**: June 11, 2026 (during Order CQRS use-case session, see ADR-014)

`AddOrderItemCommand`, `PlaceOrderCommand`, `ShipOrderCommand`, and `CancelOrderCommand`
handlers throw `Application.Common.Exceptions.NotFoundException` when
`IOrderRepository.GetByIdAsync` returns `null`. Per ADR-002, API responses should use the
`Result<T>` wrapper with appropriate `StatusCode` (404 for not-found). No API-layer
exception-handling middleware currently catches `NotFoundException` and maps it to a
`Result<T>` 404 response — like the `ValidationException` → 400 mapping above, this is
expected to be addressed together when `src/API` endpoints are added (likely the same
`IExceptionHandler`/middleware handles both).

---

## History: Resolved Decisions

### `.editorconfig` Private-Field Naming Rule vs. `_camelCase` Convention (RESOLVED - June 11, 2026)
**Decision**: Enforce bare `camelCase` for private fields, matching the existing
`.editorconfig` rule (`private_or_internal_field_should_be_camel_case` -> `camel_case` style
with empty `required_prefix`) and the existing wording in `user_preference.md` ("camelCase
for private/local variables"). No changes were made to `.editorconfig` or
`user_preference.md` — they were already correct.

Code was updated instead: renamed the non-conforming `_camelCase` private fields to
`camelCase` in `UnitOfWorkBehavior.cs` (`_unitOfWork` -> `unitOfWork`),
`ValidationBehavior.cs` (`_validators` -> `validators`), `LoggingBehavior.cs` (`_logger` ->
`logger`), `AggregateRoot.cs` (`_domainEvents` -> `domainEvents`), `Order.cs` (`_items` ->
`items`), `MartenEventStore.cs` (`_session` -> `session`, `_logger` -> `logger`),
`ApplicationDbContext.cs` (`_eventStore` -> `eventStore`, `_publisher` -> `publisher`),
`OrderRepository.cs` (`_dbContext` -> `dbContext`), and
`SqliteApplicationDbContextFactory.cs` (`_connection` -> `connection`). Also updated
`OrderConfiguration.cs`'s `SetField("_items")` -> `SetField("items")` to match the
`Order.Items` backing-field rename (required follow-on, otherwise EF Core model building
fails). Where the renamed field collided with an identically-named constructor parameter, the
constructor assignment was qualified as `this.fieldName = fieldName;` to disambiguate (does
not conflict with `dotnet_style_qualification_for_field = false`, which only flags redundant
`this.`). Verified with `dotnet build` (0 warnings/errors, IDE1006 diagnostics gone) and
`dotnet test` (41/41 passing).

The two related suggestion-severity diagnostics (IDE0290 "Use primary constructor" and CA1062
"Validate parameter is non-null on `next`") raised in the same original entry remain open and
unresolved — see the "IDE0290 'Use primary constructor'" and "CA1062 'Validate parameter is
non-null'" entries above; they are independent of this naming decision.

### Event Sourcing Strategy (RESOLVED - May 15, 2026)
**Decision**: Use **Marten** with PostgreSQL for event sourcing with Docker Compose for local development.
- See ADR-005 in `architectural_decisions.md` for full details.


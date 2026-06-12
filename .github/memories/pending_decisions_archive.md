# Pending Decisions — Resolved Archive

_Cold tier — not read by default. Consult only if you need the rationale behind a past
resolution. Open decisions live in `pending_decisions.md`._

## `.editorconfig` Private-Field Naming Rule vs. `_camelCase` Convention (RESOLVED - June 11, 2026)
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
unresolved — see `pending_decisions.md`; they are independent of this naming decision.

## Event Sourcing Strategy (RESOLVED - May 15, 2026)
**Decision**: Use **Marten** with PostgreSQL for event sourcing with Docker Compose for local development.
- See [ADR-005](adr/ADR-005-marten-event-sourcing.md) for full details.

# Pending Decisions

## Summary
Two pending items below; otherwise all major architectural decisions have been confirmed.

---

## Open: `.editorconfig` Private-Field Naming Rule vs. `_camelCase` Convention

**Raised**: June 10, 2026 (IDE diagnostics on `UnitOfWorkBehavior.cs`)

The IDE reports "Naming rule violation: Prefix '_' is not expected" (IDE1006, suggestion
severity) for private fields like `_unitOfWork`. `.editorconfig`'s
`private_or_internal_field_should_be_camel_case` style has `required_prefix = ` (empty), i.e.
it expects bare `camelCase` private fields — matching the literal wording in
`user_preference.md`'s "Naming Conventions: ... camelCase for private/local variables", but
**every** existing private field across `src/Domain`, `src/Application`, and
`src/Infrastructure` (this session's new code included) uses the `_camelCase` convention,
which is idiomatic and far more common in modern C#.

Two related suggestion-severity (non-blocking) diagnostics on the same files:
- IDE0290 "Use primary constructor" — would apply to most behavior/service classes with a
  single-purpose constructor.
- CA1062 "Validate parameter is non-null" on `Handle(..., RequestHandlerDelegate<TResponse>
  next, ...)` — MediatR guarantees `next` is non-null, so adding a defensive check would
  contradict the "don't validate what can't happen" principle.

**Not changed** in this session: renaming ~30+ existing `_field`s repo-wide (or relaxing
`.editorconfig` to `required_prefix = _`) is a sizeable, codebase-wide decision that should be
made deliberately rather than fixed ad hoc in one file. Recommendation: update
`.editorconfig`'s `private_or_internal_field_should_be_camel_case` style to
`required_prefix = _` (matching actual usage) and correct the wording in
`user_preference.md` to "`_camelCase` for private fields, `camelCase` for local variables/
parameters" — this aligns config + docs with existing code rather than the reverse.

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

## History: Resolved Decisions

### Event Sourcing Strategy (RESOLVED - May 15, 2026)
**Decision**: Use **Marten** with PostgreSQL for event sourcing with Docker Compose for local development.
- See ADR-005 in `architectural_decisions.md` for full details.


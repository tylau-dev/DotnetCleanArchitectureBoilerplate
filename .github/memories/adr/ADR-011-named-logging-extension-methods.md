# ADR-011: Named Per-Event Logging Extension Methods

**Date**: June 10, 2026
**Status**: Accepted

## Context
Code review of `LoggingBehavior` flagged direct calls to the generic
`LogInformationWithId`/`LogErrorWithId` helpers with inline log IDs and message templates
(e.g. `_logger.LogInformationWithId(2201, "Handling {RequestName}", requestName)`). This
scatters log IDs and message templates across call sites, making them hard to audit/grep and
easy to duplicate or drift.

## Decision
Generic `LogXWithId` helpers in `LoggingExtensions.cs` remain (they implement the structured
log-id prefixing convention), but call sites (behaviors, handlers, services) must not invoke
them directly. Instead, each distinct log event gets its own named extension method in the
layer's `LoggingExtensions.cs`, e.g.:
```csharp
public static void LogHandlingRequest(this ILogger logger, string requestName)
    => logger.LogInformationWithId(2201, "Handling {RequestName}", requestName);
```
`LoggingBehavior` now calls `_logger.LogHandlingRequest(requestName)`,
`_logger.LogHandledRequest(requestName)`, `_logger.LogUnhandledRequestException(ex,
requestName)`; `MartenEventStore` calls `_logger.LogAppendedDomainEvent(eventType, streamId)`.

## Rationale
- Centralizes every log ID + message template in one file per layer — easy to audit for ID
  collisions and to see the full catalog of log events a layer can emit.
- Keeps call sites self-documenting and free of magic numbers/format strings.

## Consequences
- Adding a new log event means adding a small named method to `LoggingExtensions.cs` first.
  This is a deliberate, low-cost extra step in exchange for centralization.

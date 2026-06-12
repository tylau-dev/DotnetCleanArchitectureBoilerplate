# ADR-012: DI Registration via `Extensions/ServiceExtension.cs` Per Layer

**Date**: June 10, 2026
**Status**: Accepted

## Context
`src/Application/DependencyInjection.cs` and `src/Infrastructure/DependencyInjection.cs`
(static classes named `DependencyInjection`, each exposing one `Add<Layer>` extension method)
worked but didn't follow a documented file/folder convention, and the class name shadowed the
common `Microsoft.Extensions.DependencyInjection` namespace name.

## Decision
Each layer's DI registration extension method moves to
`src/<Layer>/Extensions/ServiceExtension.cs`, namespace `<Layer>.Extensions`, static class
`ServiceExtension`. The method itself is unchanged (`AddApplication`, `AddInfrastructure`).
`src/API/Program.cs` and tests now `using Application.Extensions;` /
`using Infrastructure.Extensions;`.

## Rationale
- `Extensions/ServiceExtension.cs` is an explicit, predictable location/name for "how does this
  layer wire itself into DI" — scales cleanly if a layer ever needs more than one extension
  method (still one class, multiple methods, or split by concern under the same folder).
- Avoids a type named `DependencyInjection` colliding (in intent, if not in compiled IL) with
  the `Microsoft.Extensions.DependencyInjection` namespace it sits alongside.

## Consequences
- Future layers (e.g. a `Persistence`-only or `Messaging`-only sub-module) follow the same
  `Extensions/ServiceExtension.cs` + `Add<Thing>` pattern.

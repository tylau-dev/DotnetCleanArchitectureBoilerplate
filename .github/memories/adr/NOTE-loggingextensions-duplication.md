# Note: `LoggingExtensions` Duplication (Application / Infrastructure)

`src/Application/Common/Logging/LoggingExtensions.cs` and
`src/Infrastructure/Common/Logging/LoggingExtensions.cs` are intentionally near-identical
(~30 lines each, namespaces `Application.Common.Logging` / `Infrastructure.Common.Logging`)
implementing the project's `[LogLevel][Layer][Incremental]` structured Log ID convention
(Application = 2xxx, Infrastructure = 4xxx). Infrastructure cannot reference Application (wrong
dependency direction), so the helper is duplicated rather than shared. This is a tracked, accepted
minor duplication — a candidate for extraction into a future shared "SeedWork"/kernel package if a
third layer ever needs the same convention.

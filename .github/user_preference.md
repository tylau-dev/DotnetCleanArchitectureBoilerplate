# Development Practices & Conventions

## Code Standards

### C# Coding Style
- **Language Version**: C# 14 (supported by .NET 10)
- **Naming Conventions**: PascalCase for public members, camelCase for private/local variables
- **Access Modifiers**: Explicit (never rely on default)
- **Comments**: Minimal comments; code should be self-documenting via clear naming
  - XML comments on public contracts only
  - Complex logic deserves explanatory comments, not simple implementations
- **LINQ**: Prefer LINQ methods over query syntax for consistency
- **Async/Await**: Use `async`/`await` consistently; avoid `.Result` or `.Wait()`

### Code Organization
- **File Structure**: One public class per file (namespace mirrors folder structure)
- **Using Statements**: Sort alphabetically; use file-scoped namespaces
- **Methods**: Order by: public, protected, private; then: properties, methods, events
- **Class Size**: Aim for single responsibility; extract if >300 lines

## Architecture & Design

### Layering (Strict Separation of Concerns)
1. **Domain Layer**: Business logic, aggregates, entities, value objects, domain events
   - NO external dependencies (EF, MediatR, HttpClient, etc.)
   - Contains repository interfaces (not implementations)
2. **Application Layer**: Use cases, CQRS handlers, DTOs, pipeline behaviors
   - Depends on Domain only
   - Contains business orchestration logic
3. **Infrastructure Layer**: Persistence, external services, dependency injection
   - Implements repository interfaces
   - Configures EF DbContext, dependency injection containers
   - Contains event sourcing implementation
4. **API Layer**: HTTP endpoints, request/response mapping
   - Minimal API framework
   - Maps HTTP requests to Commands/Queries
   - Returns standardized `Result<T>` responses

### Domain-Driven Design (DDD)
- **Aggregates**: Design around aggregate roots, define boundaries
- **Entities**: Mutable objects with identity; always belong to an aggregate
- **Value Objects**: Immutable objects representing concepts; no identity
- **Domain Events**: Publish significant business occurrences; use for eventual consistency
- **Repository Pattern**: Aggregate root persistence contracts in Domain, implementations in Infrastructure
- **Specification Pattern**: Complex queries expressed as specifications

### Dependency Injection Registration
- Each layer that registers its own services for DI (Application, Infrastructure, ...) exposes
  a single static extension method on `IServiceCollection` (e.g. `AddApplication`,
  `AddInfrastructure`).
- The method lives in `src/<Layer>/Extensions/ServiceExtension.cs`, in a static class named
  `ServiceExtension`, namespace `<Layer>.Extensions`.
- The composition root (`src/API/Program.cs`) calls each layer's extension method.

### CQRS Pattern
- **Commands**: Modify state; return success/failure
- **Queries**: Retrieve data; no side effects
- **Handlers**: MediatR pipeline with cross-cutting concerns (validation, logging, etc.)
- **Pipeline Behaviors**: Validation, logging, error handling as separate behaviors
- **DTOs**: Use for data transfer; never expose domain entities to API consumers

### API Response Pattern
All API responses use standardized `Result<T>` wrapper:
```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
}
```
- Success responses: `200 OK` with data
- Validation errors: `400 Bad Request` with error messages
- Not found: `404 Not Found`
- Server errors: `500 Internal Server Error` with generic message

## Testing Strategy

### Unit Tests (Domain.Tests)
- **Focus**: Domain logic, aggregates, entities, value objects
- **Framework**: xUnit + Moq
- **Coverage Target**: Core business logic (80%+)
- **Naming**: `Should_[Expectation]_When_[Condition]`
- **Arrange-Act-Assert**: Clear test structure
- **No Database**: In-memory or mocked dependencies

### Integration Tests (Application.Tests)
- **Focus**: CQRS handlers, application services, domain event propagation
- **Framework**: xUnit with test fixtures and in-memory database
- **Coverage Target**: Happy paths and error scenarios
- **Database**: In-memory EF Core for isolation
- **Event Verification**: Confirm domain events are raised and propagated

### Test Organization
- Mirror source structure: `tests/[Layer].Tests/` matches `src/[Layer]/`
- Fixtures: Reusable test data builders and database setup
- Helper Methods: Common assertions and setup logic

## Dependency Management

### NuGet Package Guidelines
- **Directory.Build.props**: Shared across all projects
  - All shared packages with explicit versions
  - Testing packages separated from application packages
- **Individual .csproj**: Project-specific packages not in Directory.Build.props
- **Version Strategy**: Use latest stable versions; check for vulnerabilities via `dotnet list package --vulnerable`
- **Minimal Dependencies**: Only add packages that solve real problems

### Recommended Packages (Confirmed)
- **MediatR**: CQRS command/query dispatching
- **Entity Framework Core**: Database access and migrations
- **FluentValidation**: Command/query validation
- **AutoMapper**: DTO mapping
- **xUnit**: Unit and integration testing
- **Moq**: Mocking framework

### Pending Packages (Event Sourcing)
- **Marten** OR **EventStore.Client**: Event sourcing (decision pending)

## Logging Strategy

### LoggingExtensions Pattern
All logging uses custom `LoggingExtensions` for consistency and structured logging.

### Call-Site Convention
- Application/Infrastructure code (behaviors, handlers, services) must **not** call
  `LogXWithId(...)` directly with inline messages and log IDs.
- Each distinct log event gets its own named extension method in the layer's
  `LoggingExtensions.cs` (e.g. `LogHandlingRequest`, `LogAppendedDomainEvent`), which itself
  wraps the generic `LogXWithId` helper with the fixed log ID and message template.
- This keeps log IDs and message templates centralized and discoverable in one file per layer,
  and keeps call sites readable (`_logger.LogHandlingRequest(name)` instead of
  `_logger.LogInformationWithId(2201, "Handling {RequestName}", name)`).

### Log ID Format
Log IDs follow a two-plus-digit format: `[LogLevel][DomainLayer][Incremental]`

**First Digit - Log Level**:
- `1` = Information
- `2` = Warning
- `3` = Error
- `4` = Critical
- `5` = Debug
- `6` = Trace

**Second Digit - Domain Layer**:
- `1` = API Layer
- `2` = Application Layer
- `3` = Domain Layer
- `4` = Infrastructure Layer
- `5` = Cross-cutting Concerns

**Remaining Digits - Incremental**: Sequential numbering within each category (01, 02, 03, etc.)

### Examples
- `1101` = Information from API Layer, event 01
- `2203` = Warning from Application Layer, event 03
- `3401` = Error from Infrastructure Layer, event 01
- `4302` = Critical from Domain Layer, event 02

### Implementation
```csharp
public static class LoggingExtensions
{
    public static void LogInformation(this ILogger logger, string logId, string message, params object[] args)
        => logger.LogInformation($"[{logId}] {message}", args);
    
    // Similar patterns for Warning, Error, Critical, Debug, Trace
}
```

### Usage Example
```csharp
_logger.LogInformation("1101", "API request received for {Endpoint}", endpoint);
_logger.LogError("3401", "Repository query failed: {Exception}", ex);
```

## Configuration Management

### Settings & Secrets
- **Configuration**: Use `appsettings.json` + `appsettings.{Environment}.json`
- **Development Secrets**: Store in `appsettings.Development.json` (local, never committed)
- **Production Secrets**: Use environment variables or secure secret management
- **.gitignore**: Include `appsettings.Development.json` and `.env` to prevent secret leaks
- **No Hardcoding**: Zero tolerance for hardcoded secrets, API keys, or connection strings —
  including local-development defaults and values used only in tests
- **Docker Compose Secrets**: `docker-compose.yml` must not contain hardcoded credentials.
  Local credentials are supplied via a gitignored `.env` file (copied from a committed
  `.env.example` template); docker-compose substitutes `${VAR}` references from `.env`
  automatically
- **Test Connection Strings**: Even non-functional placeholder connection strings used only to
  satisfy DI-registration checks in tests belong in an `appsettings.json` file, not hardcoded
  inline in test source

## Memory File Management

### Work Log Strategy
- A new `work_log_[DATE]_[BRANCH].md` file is created for each work session
  - Format: `work_log_YYYY-MM-DD_branch-name.md`
  - Example: `work_log_2026-05-15_feature-prompt-initiation.md`
- If work continues on the same date/branch, update the existing session file
- Session files are kept in `.github/memories/` for historical reference

### File Structure
Each work session file includes:
- Date and branch name in header
- List of completed items
- Decisions made
- Blockers or issues
- Next steps

## Git Workflow

### Branch Strategy
- `feature/prompt-initiation`: Current branch for creating shared prompts and initial structure
- `feature/*`: Feature branches for new functionality
- `bugfix/*`: Bug fix branches
- `main` or `master`: Production-ready code

### Commit Messages
- **Format**: `[Type] Brief description` or `[Type](Scope): Brief description`
- **Types**: feat, fix, docs, style, refactor, test, chore
- **Example**: `[feat](domain): Add aggregate root base class`

### Pre-commit Checks
- Code compiles without warnings
- Tests pass locally before pushing
- No sensitive data in commits
- Code follows established standards

## Documentation

### Code Documentation
- **README.md**: Project overview, setup instructions, running the app
- **ARCHITECTURE.md**: Architecture overview, layer descriptions, design patterns, decision rationale
- **CONTRIBUTION.md**: Guidelines for contributing to the project
- **Inline Comments**: Explain "why" not "what"; complex logic only

### Memory Files
- `.github/memories/work_log.md`: Chronological work log
- `.github/memories/architectural_decisions.md`: ADRs (Architecture Decision Records)
- `.github/memories/pending_decisions.md`: Outstanding decisions and blockers

## Validation & Quality Gates

### Before Committing
- ✅ Code compiles without errors or warnings
- ✅ All tests pass locally
- ✅ Code follows established conventions
- ✅ No security vulnerabilities introduced
- ✅ No sensitive data in code

### Build & Test Commands
```bash
# Restore and build
dotnet build

# Run all tests
dotnet test

# Check for vulnerabilities
dotnet list package --vulnerable

# Code style/analysis
# (Enable via Directory.Build.props)
```

## Environment Setup

### Required Tools
- .NET 10 SDK
- Git
- Code editor: VS Code or Visual Studio
- Terminal: Bash/Zsh/PowerShell

### Project Setup
```bash
cd /Users/tylau/Documents/DevProjects/DotnetCleanCodeBoilerplate
dotnet build
dotnet test
```

## Review Checklist

Before marking work as complete:
- [ ] Code compiles without warnings
- [ ] All tests pass (unit + integration)
- [ ] Code follows established conventions
- [ ] XML comments added to public contracts (if applicable)
- [ ] No hardcoded secrets or sensitive data
- [ ] Architecture layers respected
- [ ] Memory files updated (work_log.md, current_state.md)
- [ ] Documentation reflects changes

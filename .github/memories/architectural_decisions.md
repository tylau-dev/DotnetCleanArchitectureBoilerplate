# Architectural Decisions

## ADR-001: Clean Architecture + DDD + CQRS Pattern

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Building a professional-grade .NET boilerplate project that serves as a reference for future applications.

### Decision
Implement Clean Architecture with Domain-Driven Design (DDD) and Command Query Responsibility Segregation (CQRS) patterns.

### Rationale
- **Clean Architecture**: Provides clear separation of concerns across Domain, Application, Infrastructure, and API layers
- **DDD**: Enables modeling complex business logic through aggregates, entities, value objects, and domain events
- **CQRS**: Separates read (Query) and write (Command) concerns for scalability and maintainability

### Consequences
- Increased initial development time due to layer separation
- Improved testability and maintainability
- Better alignment with enterprise patterns
- Easier to adapt for various business domains

---

## ADR-002: Standardized Result<T> API Response Wrapper

**Date**: May 15, 2026  
**Status**: Accepted

### Context
API endpoints need consistent response handling for success, validation errors, and server errors.

### Decision
Use standardized `Result<T>` wrapper for all API responses with properties: Success, Data, Errors, StatusCode.

### Rationale
- Provides consistent API contract across all endpoints
- Simplifies client-side error handling
- Enables centralized error serialization
- Supports validation error details

### Consequences
- All endpoints must conform to Result<T> pattern
- Requires custom response serialization
- Better client predictability

---

## ADR-003: .NET 10 as Target Framework

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Choosing between .NET 10 (latest, cutting-edge) vs .NET 8 LTS (stable, long-term support).

### Decision
Use .NET 10 as the target framework.

### Rationale
- Latest language features and performance improvements
- Demonstrates modern C# capabilities (C# 13)
- Still on LTS-adjacent release cycle with regular updates
- Boilerplate serves as learning resource; newer is better for that purpose

### Consequences
- Must maintain updated with .NET 10 releases
- Some enterprise constraints may limit adoption
- Access to latest language features

---

## ADR-004: Separate Unit and Integration Test Projects

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Tests need clear organization with equal emphasis on unit tests (domain logic) and integration tests (CQRS handlers).

### Decision
Create two separate test projects:
- `tests/Domain.Tests/` - Unit tests for domain logic
- `tests/Application.Tests/` - Integration tests for application handlers

### Rationale
- Clear separation of test responsibilities
- Enables focused test execution
- Better organization for complex test fixtures
- Easier to maintain and extend

### Consequences
- More test projects to maintain
- Clear build and test execution paths
- Better test organization and discovery

---

## ADR-005: Marten for Event Sourcing with Docker Support

**Date**: May 15, 2026  
**Status**: Accepted

### Context
Event sourcing is core to the infrastructure layer. Two primary options existed: Marten (PostgreSQL-based) and EventStore.Client (dedicated).

### Decision
Use **Marten** with PostgreSQL for event sourcing. Implement Docker + Docker Compose for local development using official PostgreSQL image.

### Rationale
- Marten simplifies event sourcing integration with PostgreSQL
- Docker Compose enables consistent local development environment
- PostgreSQL official image ensures stability and security updates
- Reduces operational complexity compared to dedicated event store
- Better integration with Entity Framework Core for standard ORM needs
- Excellent support for CQRS patterns and domain events

### Implementation Details
- **Event Store**: Marten
- **Database**: PostgreSQL (official Docker image)
- **Orchestration**: Docker Compose for local development
- **Connection**: configurable via appsettings.Development.json
- **Infrastructure**: Docker + docker-compose.yml in project root

### Consequences
- PostgreSQL dependency required for local and production environments
- Docker required for local development (documented in CONTRIBUTION.md)
- Marten package added to Directory.Build.props
- Infrastructure layer manages Marten configuration and migrations

---

## ADR-006: Development Practices Documentation

**Date**: May 15, 2026  
**Status**: Accepted

### Decision
Comprehensive development practices documented in `.github/user_preference.md` covering:
- Code standards and organization
- Architecture enforcement
- Testing strategy
- Configuration management
- Git workflow
- Quality gates and validation

### Rationale
- Ensures consistency across development
- Provides onboarding reference
- Documents expectations clearly
- Prevents architectural drift

### Consequences
- Clear guidelines for implementation
- Must maintain consistency with documented practices

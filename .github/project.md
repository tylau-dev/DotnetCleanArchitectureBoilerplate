# Project Information: .NET Clean Code Boilerplate

## Overview
This is a **purely technical reference project** demonstrating professional-grade .NET application architecture using Clean Architecture, Domain-Driven Design (DDD), and CQRS patterns. It serves as a boilerplate for future projects and as a learning resource for implementing enterprise-grade .NET applications.

## Project Scope
This boilerplate is not a domain-specific business application. Instead, it provides:
- A well-structured solution template with proper layer separation
- Example implementations of DDD patterns (aggregates, entities, value objects, domain events)
- CQRS pattern demonstration with MediatR
- Comprehensive test coverage (unit and integration tests)
- Production-ready API structure with standardized response handling
- Clean separation of concerns across layers
- Best practices and patterns documentation

## Business/Functional Context
**Status**: This is a reference implementation project.

As this is a **purely technical boilerplate project**, functional/business requirements are minimal. Instead, the project demonstrates:

1. **Example Domain**: A simple domain model (to be determined) used solely to demonstrate DDD and CQRS patterns
2. **Reference Architecture**: Serves as a template for future business-domain projects
3. **Pattern Library**: Provides reusable patterns and implementations

### Future Business Domains
When this boilerplate is adapted for actual business requirements, this file will be updated to include:
- Specific business goals and objectives
- Domain model overview and core business rules
- Key features and use cases
- Stakeholder requirements
- Success metrics

## Technical Decisions

### Confirmed Decisions
- **.NET Version**: .NET 10 (latest)
- **Architecture**: Clean Architecture + DDD + CQRS
- **API Response Pattern**: Standardized `Result<T>` wrapper
- **Testing Strategy**: Equal emphasis on unit tests and integration tests
- **Documentation**: README.md, ARCHITECTURE.md, and contribution guidelines included

### Confirmed Technical Stack
- **Event Sourcing**: Marten with PostgreSQL
  - Docker + Docker Compose for local development
  - Official PostgreSQL image for consistency
  - Simplifies event sourcing integration

## Success Criteria
- ✅ Clean, maintainable, production-grade code
- ✅ Comprehensive test coverage (unit + integration)
- ✅ Clear separation of concerns across all layers
- ✅ Well-documented architecture and patterns
- ✅ Reusable for future projects
- ✅ Demonstrates SOLID principles and enterprise best practices

## Related Documentation
- See `.github/agent.md` for agent identity and workspace context
- See `.github/user_preference.md` for development practices and conventions
- See `.github/memories/` for work log and architectural decisions

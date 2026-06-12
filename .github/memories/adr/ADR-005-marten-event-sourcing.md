# ADR-005: Marten for Event Sourcing with Docker Support

**Date**: May 15, 2026
**Status**: Accepted

## Context
Event sourcing is core to the infrastructure layer. Two primary options existed: Marten (PostgreSQL-based) and EventStore.Client (dedicated).

## Decision
Use **Marten** with PostgreSQL for event sourcing. Implement Docker + Docker Compose for local development using official PostgreSQL image.

## Rationale
- Marten simplifies event sourcing integration with PostgreSQL
- Docker Compose enables consistent local development environment
- PostgreSQL official image ensures stability and security updates
- Reduces operational complexity compared to dedicated event store
- Better integration with Entity Framework Core for standard ORM needs
- Excellent support for CQRS patterns and domain events

## Implementation Details
- **Event Store**: Marten
- **Database**: PostgreSQL (official Docker image)
- **Orchestration**: Docker Compose for local development
- **Connection**: configurable via appsettings.Development.json
- **Infrastructure**: Docker + docker-compose.yml in project root

## Consequences
- PostgreSQL dependency required for local and production environments
- Docker required for local development (documented in CONTRIBUTION.md)
- Marten package added to Directory.Build.props
- Infrastructure layer manages Marten configuration and migrations

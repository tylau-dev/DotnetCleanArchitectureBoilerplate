# ADR-004: Separate Unit and Integration Test Projects

**Date**: May 15, 2026
**Status**: Accepted

## Context
Tests need clear organization with equal emphasis on unit tests (domain logic) and integration tests (CQRS handlers).

## Decision
Create two separate test projects:
- `tests/Domain.Tests/` - Unit tests for domain logic
- `tests/Application.Tests/` - Integration tests for application handlers

## Rationale
- Clear separation of test responsibilities
- Enables focused test execution
- Better organization for complex test fixtures
- Easier to maintain and extend

## Consequences
- More test projects to maintain
- Clear build and test execution paths
- Better test organization and discovery

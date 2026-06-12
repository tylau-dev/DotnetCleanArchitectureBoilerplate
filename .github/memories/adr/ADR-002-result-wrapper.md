# ADR-002: Standardized Result<T> API Response Wrapper

**Date**: May 15, 2026
**Status**: Accepted

## Context
API endpoints need consistent response handling for success, validation errors, and server errors.

## Decision
Use standardized `Result<T>` wrapper for all API responses with properties: Success, Data, Errors, StatusCode.

## Rationale
- Provides consistent API contract across all endpoints
- Simplifies client-side error handling
- Enables centralized error serialization
- Supports validation error details

## Consequences
- All endpoints must conform to Result<T> pattern
- Requires custom response serialization
- Better client predictability

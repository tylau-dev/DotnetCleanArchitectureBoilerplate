# ADR-013: Local PostgreSQL Secrets via `.env` (Docker Compose)

**Date**: June 10, 2026
**Status**: Accepted

## Context
The original `docker-compose.yml` hardcoded `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD`
as committed plaintext (labeled "development defaults only"), and
`ApplicationDbContextFactory` carried a matching hardcoded `DefaultConnectionString` fallback
containing the same credentials. Code review flagged both as violating the "no hardcoded
secrets, including dev defaults" rule in the Review Checklist.

## Decision
- `docker-compose.yml` no longer hardcodes Postgres credentials; it references
  `${POSTGRES_DB}`, `${POSTGRES_USER}`, `${POSTGRES_PASSWORD}`, `${POSTGRES_PORT:-5432}`, which
  docker-compose resolves from a gitignored `.env` file at the repo root.
- A committed `.env.example` documents the expected variables (with the same values previously
  hardcoded, now as a *template* a developer copies to `.env`).
- `.gitignore` now excludes `.env` alongside `appsettings.Development.json`.
- `ApplicationDbContextFactory.CreateDbContext` no longer has a `DefaultConnectionString`
  fallback. If `ConnectionStrings:Database` isn't found via `appsettings.json` /
  `appsettings.Development.json` / environment variables (`AddEnvironmentVariables()` added),
  it throws `InvalidOperationException` with setup instructions referencing `.env.example` and
  `appsettings.Development.json`.
- `tests/Infrastructure.Tests/appsettings.json` (committed) holds a non-functional placeholder
  connection string (`Host=localhost;...;Username=test;Password=test`) used only so
  `AddInfrastructure`'s configuration check passes during DI-registration tests — no real
  connection is opened, so this is not a secret and needs no `appsettings.Development.json`
  counterpart.

## Rationale
- Removes the only two places a real-looking Postgres credential pair was committed to source
  control, while keeping local setup a one-line `cp .env.example .env`.
- A hard failure with actionable guidance is preferable to silently falling back to a
  credential pair that may or may not match the developer's actual local database.

## Consequences
- First-time setup requires `cp .env.example .env` (and creating
  `src/API/appsettings.Development.json` per ADR-009) before `docker-compose up -d` /
  `dotnet ef database update` will work — documented in the work log as a manual step.

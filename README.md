# .NET Clean Code Boilerplate

A professional-grade .NET reference project demonstrating **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** patterns for building maintainable, scalable, and testable applications.

## 🎯 What This Project Does

This is a **technical boilerplate repository** designed to showcase industry best practices for .NET application architecture. It provides a production-ready template with properly layered code, comprehensive test coverage, and clear separation of concerns—perfect for use as a reference or starting point for new enterprise .NET projects.

## ✨ Why Use This Boilerplate?

- **Clean Architecture** - Properly layered solution (Domain → Application → Infrastructure → API)
- **Domain-Driven Design** - DDD patterns including aggregates, entities, value objects, and domain events
- **CQRS Pattern** - Command Query Responsibility Segregation with MediatR for clean separation
- **Event Sourcing** - Marten-based event store with PostgreSQL for event-driven architectures
- **Comprehensive Testing** - Unit and integration test examples demonstrating CQRS testing patterns
- **Production-Ready Structure** - Standardized response handling, validation, logging, and error management
- **Docker Integration** - Local development environment with Docker Compose (PostgreSQL included)
- **Code Quality** - StyleCop and FxCop analysis for consistent code standards

## 🚀 Quick Start

### Prerequisites

- **.NET 10 SDK** or later ([download](https://dotnet.microsoft.com/download))
- **Docker & Docker Compose** (for local PostgreSQL with event store)
- A code editor (Visual Studio, VS Code, or Rider)

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd DotnetCleanCodeBoilerplate
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Start the local development environment:**
   ```bash
   docker-compose up -d
   ```

4. **Build the solution:**
   ```bash
   dotnet build
   ```

5. **Run tests to verify setup:**
   ```bash
   dotnet test
   ```

### Project Structure

```
DotnetCleanCodeBoilerplate/
├── src/
│   ├── Domain/              # Domain layer - core business logic, entities, aggregates
│   ├── Application/         # Application layer - CQRS handlers, DTOs, use cases
│   ├── Infrastructure/      # Infrastructure layer - persistence, event store, DI setup
│   └── API/                 # API layer - HTTP endpoints, minimal API
├── tests/
│   ├── Domain.Tests/        # Unit tests for domain logic
│   └── Application.Tests/   # Integration tests for CQRS handlers
├── .github/
│   ├── AGENT.md            # Agent identity and responsibilities
│   ├── project.md          # Project context and decisions
│   ├── user_preference.md  # Development standards and conventions
│   ├── memories/           # Work logs and architectural decisions
│   └── prompts/            # Reusable AI prompts for development
├── docker-compose.yml      # Local PostgreSQL + event store
└── README.md               # This file
```

## 📚 Architecture Overview

### Layered Architecture

```
┌─────────────────────────────┐
│     API Layer               │  HTTP endpoints, Minimal API
├─────────────────────────────┤
│   Application Layer         │  CQRS, DTOs, Use Cases
├─────────────────────────────┤
│   Infrastructure Layer      │  Database, Event Store, DI
├─────────────────────────────┤
│     Domain Layer            │  Entities, Aggregates, Value Objects
└─────────────────────────────┘
```

### Key Patterns Used

- **CQRS**: Commands modify state, Queries retrieve data (handled by MediatR)
- **DDD**: Aggregates, entities, value objects, and domain events
- **Event Sourcing**: Marten event store for event-driven workflows
- **Repository Pattern**: Domain-driven persistence abstractions
- **Result<T> Pattern**: Standardized API response wrapper for success/error handling

### Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| API | Minimal API (.NET) | Lightweight HTTP framework |
| CQRS | MediatR | Command/Query dispatching |
| Domain Events | Marten | Event sourcing foundation |
| Data | Entity Framework Core | ORM for repositories |
| Event Store | Marten | PostgreSQL-based event store |
| Validation | FluentValidation | Business rule validation |
| Mapping | AutoMapper | DTO ↔ Entity mapping |
| Testing | xUnit + Moq | Unit and integration tests |
| Analysis | StyleCop, FxCop | Code quality enforcement |

## 💻 Usage Examples

### Running the Application

```bash
dotnet run --project src/API
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test tests/Domain.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Stopping Local Development Environment

```bash
docker-compose down
```

## 📖 Documentation

For more detailed information, see:

- [.github/project.md](.github/project.md) - Project context and technical decisions
- [.github/user_preference.md](.github/user_preference.md) - Development standards and code conventions

## 🤝 Contributing

We welcome contributions! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Development setup instructions
- Code style guidelines
- Testing requirements
- Pull request process

## 📝 Development Standards

This project enforces consistent code quality through:

- **Code Style**: StyleCop and FxCop analysis
- **Naming**: PascalCase for public members, camelCase for private/locals
- **Comments**: Self-documenting code with XML docs on public contracts only
- **Testing**: Unit tests (domain logic) + integration tests (CQRS handlers)
- **Async/Await**: Consistent async patterns; no `.Result` or `.Wait()`

See [.github/user_preference.md](.github/user_preference.md) for complete conventions.

## 🐛 Support & Help

For issues or questions:

1. **Check existing documentation** - Start with [ARCHITECTURE.md](ARCHITECTURE.md)
2. **Review test examples** - Look at test projects for usage patterns
3. **Check GitHub Issues** - Search for similar issues
4. **Reference code patterns** - Browse the Domain, Application, and Infrastructure layers

## 📋 License

This project is provided as a reference boilerplate. See LICENSE file for details.

## 👤 Maintainers

This is a technical reference project maintained by the development team. For questions about architecture or design patterns, refer to [ARCHITECTURE.md](ARCHITECTURE.md) and the [.github/memories](.github/memories) directory for decision logs.

---

**Getting Started?** Begin with the [Quick Start](#-quick-start) section above, then explore the test projects to understand CQRS and DDD patterns in action.

# ADR-001: Application Architecture

**Status:** Accepted  
**Date:** 2025-12-09  
**Decision Makers:** Azure Naming Tool Team  
**Version:** 5.0.0

## Context

The Azure Naming Tool is a web-based application designed to help administrators define and manage Azure resource naming conventions while providing a simple interface for users to generate compliant names. The tool needed an architecture that supports:

- Modern web UI with interactive components
- RESTful API for programmatic access
- Flexible configuration management
- Both human and machine interaction patterns
- Scalability for enterprise use
- Easy deployment across multiple hosting platforms

## Decision

We have adopted a **Blazor Server-based architecture** using ASP.NET Core with the following key architectural components:

### Technology Stack
- **.NET 10.0** - Latest LTS framework for performance, security, and modern features
- **Blazor Server** - Interactive server-rendered UI with SignalR for real-time communication
- **ASP.NET Core Web API** - RESTful API endpoints for programmatic access
- **Entity Framework Core** - Data access layer with provider abstraction
- **Repository Pattern** - Abstraction layer for storage providers

### Architectural Layers

#### 1. Presentation Layer
- **Blazor Components** (`Components/`) - Interactive UI built with Razor components
  - Server-side rendering with WebSocket (SignalR) connectivity
  - Component-based architecture for reusability
  - Modal dialogs using Blazored.Modal
  - Toast notifications using Blazored.Toast
  - State management via StateContainer singleton

#### 2. API Layer
- **REST Controllers** (`Controllers/`) - HTTP endpoints for external integration
  - API Versioning (v1 and v2) with backward compatibility
  - Swagger/OpenAPI documentation
  - API key authentication via custom attributes
  - Rate limiting middleware
  - Correlation ID tracking for distributed tracing

#### 3. Business Logic Layer
- **Services** (`Services/`) - Core business logic and orchestration
  - `AzureValidationService` - Azure tenant name validation
  - `StorageMigrationService` - Data migration between providers
  - Service interfaces for dependency injection and testing

#### 4. Data Access Layer
- **Repositories** (`Repositories/`) - Abstract storage operations
  - Interface-based design (`Repositories/Interfaces/`)
  - Multiple implementations:
    - `SQLiteStorageProvider` - Entity Framework Core with SQLite
    - `FileSystemStorageProvider` - JSON file-based storage
  - Repository pattern isolates storage concerns from business logic

#### 5. Domain Layer
- **Models** (`Models/`) - Domain entities and data transfer objects
  - Configuration entities (ResourceType, ResourceLocation, etc.)
  - Request/Response DTOs for API
  - Validation attributes

#### 6. Cross-Cutting Concerns
- **Helpers** (`Helpers/`) - Utility functions
  - CacheHelper - In-memory caching
  - ConfigurationHelper - Settings management
  - FileSystemHelper - File operations
  - ValidationHelper - Input validation
  - LogHelper - Structured logging
- **Middleware** (`Middleware/`) - Request pipeline components
  - ApiLoggingMiddleware - Request/response logging with sanitization
  - CorrelationIdMiddleware - Distributed tracing support
- **Health Checks** (`HealthChecks/`) - Application health monitoring
  - CacheHealthCheck - Memory cache status
  - StorageHealthCheck - Database/file system connectivity

### Design Patterns

1. **Repository Pattern** - Abstracts data access, allowing storage provider swapping
2. **Dependency Injection** - All services registered via .NET DI container
3. **Options Pattern** - Configuration bound to strongly-typed classes
4. **Strategy Pattern** - Multiple storage providers (SQLite, FileSystem)
5. **API Versioning** - Backwards-compatible API evolution (v1, v2)

### API Architecture

The application exposes two API versions:

- **API v1.0** - Current stable API with backward compatibility
  - Traditional controller endpoints
  - Single resource operations
  
- **API v2.0** - Enhanced API with modern features
  - Bulk operations support
  - Enhanced filtering and sorting
  - Improved error responses with detailed messages
  - Consistent response format with ApiResponse wrapper

API endpoints support:
- JSON request/response with camelCase naming
- Enum serialization as strings (human-readable)
- OpenAPI/Swagger documentation
- API key authentication
- Rate limiting and throttling

## Consequences

### Positive

✅ **Modern Development Experience**
- .NET 10.0 provides latest language features and performance improvements
- Hot reload support for rapid development
- Strong typing throughout the application

✅ **Scalability**
- Blazor Server scales well with SignalR load balancing
- Repository pattern allows easy migration to different storage backends
- API versioning enables non-breaking feature additions

✅ **Maintainability**
- Clear separation of concerns across layers
- Dependency injection simplifies testing and mocking
- Interface-based design allows implementation swapping

✅ **Flexibility**
- Multiple storage providers (SQLite for performance, FileSystem for simplicity)
- Multiple authentication methods (API keys, configurable in future)
- Dual access patterns (UI and API)

✅ **Developer Productivity**
- Blazor components enable rapid UI development
- Shared C# code between client and server logic
- Comprehensive Swagger documentation for API consumers

### Negative

⚠️ **Blazor Server Limitations**
- Requires persistent WebSocket connection (SignalR)
- Not suitable for offline scenarios
- Higher server resource usage compared to static sites
- Network latency affects UI responsiveness

⚠️ **Complexity**
- Multiple abstraction layers may be overkill for small deployments
- Repository pattern adds indirection
- Dual storage provider support requires careful testing

⚠️ **Dependencies**
- Requires .NET 10.0 runtime (breaking change from .NET 8.0)
- SQLite native libraries required for database provider
- SignalR requires WebSocket support in hosting environment

### Mitigations

- Circuit timeouts and retry logic configured for unreliable networks
- Storage provider abstraction allows fallback to FileSystem
- Comprehensive health checks detect configuration issues early
- Migration tools help users upgrade between storage providers

## Alternatives Considered

### 1. Blazor WebAssembly
**Rejected** - Would require duplicating storage logic on client, no server-side validation benefits, larger initial download size.

### 2. Traditional MVC with JavaScript
**Rejected** - Less interactive UI, more complex state management, dual language maintenance (C# + JS).

### 3. Single Storage Provider (SQLite only)
**Rejected** - Would break backward compatibility for existing deployments, removes deployment flexibility.

### 4. Microservices Architecture
**Rejected** - Over-engineered for current scale, unnecessary operational complexity, single-application scope doesn't justify distributed architecture.

## References

- [Microsoft Cloud Adoption Framework - Naming Conventions](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging)
- [Blazor Server Documentation](https://learn.microsoft.com/aspnet/core/blazor/hosting-models#blazor-server)
- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [Repository Pattern in .NET](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

## Related ADRs

- [ADR-002: Hosting Architecture](ADR-002-hosting-architecture.md)
- [ADR-003: Data Storage Architecture](ADR-003-data-storage-architecture.md)

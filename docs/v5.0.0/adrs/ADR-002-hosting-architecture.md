# ADR-002: Hosting Architecture

**Status:** Accepted  
**Date:** 2025-12-09  
**Decision Makers:** Azure Naming Tool Team  
**Version:** 5.0.0

## Context

The Azure Naming Tool needs to support diverse deployment scenarios across different organizations with varying infrastructure preferences, security requirements, and operational capabilities. The hosting solution must support:

- **Enterprise deployments** - Large organizations with existing Azure infrastructure
- **Small/medium businesses** - Organizations with limited cloud resources
- **Development/testing** - Rapid local deployment for development teams
- **Air-gapped environments** - Disconnected networks with strict security requirements
- **Multi-region deployments** - Global organizations requiring regional instances
- **Minimal operational overhead** - Easy deployment and maintenance

## Decision

We have adopted a **multi-platform hosting strategy** supporting four primary deployment models:

### 1. Azure App Service (Recommended for Production)

**Target:** Organizations with Azure subscriptions seeking managed PaaS deployment

**Architecture:**
- ASP.NET Core web application deployed to Azure App Service
- .NET 10.0 runtime on Linux or Windows
- Persistent storage via mounted volumes or Azure Storage
- Application Insights for monitoring and diagnostics
- Auto-scaling based on load
- Built-in SSL/TLS certificates via Azure
- Managed identity support for Azure service authentication

**Deployment Methods:**
- GitHub Actions workflow (OIDC or publish profile authentication)
- Azure CLI deployment
- ARM/Bicep templates
- Terraform configurations

**Storage Options:**
- SQLite database on persistent storage (Premium/Isolated tiers with persistent disk)
- JSON files on App Service file system
- Future: Azure SQL Database support for high availability

**Authentication:**
- Azure Managed Identity for Azure Tenant Name Validation
- Service Principal authentication (alternative)
- API key authentication for API access

**Advantages:**
- Fully managed platform (no OS patching)
- Auto-scaling and high availability
- Built-in monitoring and diagnostics
- Azure ecosystem integration
- Easy OIDC authentication setup

**Considerations:**
- Requires Azure subscription
- Cold start delays on free/shared tiers
- Persistent storage required for SQLite (Premium tier+)
- WebSocket support required for Blazor Server (enabled by default)

### 2. Docker Container (Recommended for On-Premises)

**Target:** Organizations with container orchestration platforms or self-hosted environments

**Architecture:**
- Multi-stage Docker build (build → publish → runtime)
- Base image: `mcr.microsoft.com/dotnet/aspnet:8.0` (Note: Will update to 10.0)
- Exposed ports: 8080 (HTTP), 8081 (HTTPS)
- Volume mounts for persistent data storage
- Environment variable configuration
- Health check endpoints for orchestration

**Deployment Platforms:**
- Docker standalone
- Docker Compose
- Kubernetes / AKS
- Azure Container Instances
- AWS ECS / Fargate
- On-premises container platforms

**Configuration:**
- Environment variables override appsettings.json
- Volume mount for `/app/settings` directory
- SQLite database stored on persistent volume
- Configuration via docker-compose.yml or Kubernetes manifests

**Advantages:**
- Platform agnostic (runs anywhere Docker is supported)
- Consistent environment across dev/test/prod
- Easy scaling in orchestration platforms
- Small image size (~200MB)
- Isolated runtime environment

**Considerations:**
- Requires container runtime environment
- Persistent volumes required for data retention
- Network configuration for WebSocket support
- Container orchestration knowledge helpful

### 3. Windows/Linux Standalone

**Target:** Development teams, testing environments, or organizations without cloud/container infrastructure

**Architecture:**
- Self-contained .NET application
- Kestrel web server (built-in)
- File-based or SQLite storage
- Manual service configuration (systemd, Windows Service)
- Reverse proxy optional (IIS, nginx, Apache)

**Deployment:**
- Compiled binaries (dotnet publish)
- Manual installation to target server
- Configuration via appsettings.json
- systemd service (Linux) or Windows Service (Windows)

**Advantages:**
- No external dependencies (cloud or containers)
- Complete control over hosting environment
- Lowest cost (use existing hardware)
- Air-gapped deployment support
- Simple backup/restore (copy files)

**Considerations:**
- Manual updates and patching required
- No built-in scaling capabilities
- OS-level security management required
- TLS/SSL certificates managed manually

### 4. Azure Container Instances (ACI)

**Target:** Organizations wanting serverless containers without orchestration complexity

**Architecture:**
- Container deployed to Azure Container Instances
- Public or private networking
- Volume mounts via Azure Files or Empty Dir
- Integration with Azure Virtual Networks
- Managed identity support

**Advantages:**
- Serverless container deployment
- Pay-per-second billing
- Fast startup times
- No cluster management
- Simple deployment model

**Considerations:**
- Limited to Azure platform
- No built-in load balancing (requires Azure Front Door/App Gateway)
- Persistent storage via Azure Files (slower than managed disks)

## Configuration Management

All deployment models support consistent configuration via:

1. **appsettings.json** - Base configuration file
2. **Environment Variables** - Override settings (12-factor app pattern)
3. **User Secrets** - Development-only secrets management
4. **Azure App Configuration** - Centralized configuration (future)
5. **Docker Secrets** - Container orchestration secret management

### Key Configuration Areas

```json
{
  "StorageProvider": "SQLite",           // or "FileSystem"
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=..."
  },
  "AzureValidation": {
    "Enabled": true,
    "AuthMode": "ManagedIdentity",      // or "ServicePrincipal"
    "Strategy": "NotifyOnly"            // or "AutoIncrement", "Fail", "SuffixRandom"
  },
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
```

## Networking Requirements

All deployment models require:

- **HTTP/HTTPS** - Ports 80/443 or custom (8080/8081 in containers)
- **WebSocket Support** - Blazor Server requires persistent WebSocket connections
- **Outbound HTTPS** (optional) - For Azure Tenant Name Validation feature
- **Load Balancer Affinity** - Session affinity/sticky sessions required for multi-instance deployments

## Monitoring and Observability

### Application Insights (Azure)
- Automatic telemetry collection
- Performance monitoring
- Exception tracking
- Custom metrics and logs
- Dependency tracking (Azure API calls)

### Health Checks

The application implements comprehensive health check endpoints for monitoring and orchestration:

#### Endpoints

**1. `/healthcheck/ping`** (Basic Health Check)
- Simple 200 OK response if application is running
- No dependency checks
- Backward compatibility endpoint
- Use for: Basic uptime monitoring

**2. `/health/live`** (Liveness Probe)
- Returns 200 OK if application is alive
- Does not execute dependency checks
- Fast response (< 10ms)
- Use for: Kubernetes liveness probes, container orchestration
- Purpose: Detect if application needs restart

**3. `/health/ready`** (Readiness Probe)
- Comprehensive readiness check with JSON response
- Executes all health checks tagged with "ready"
- Includes detailed status per component
- Use for: Kubernetes readiness probes, load balancer health checks
- Purpose: Determine if application can handle traffic

**Response Format (`/health/ready`):**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "storage",
      "status": "Healthy",
      "description": "Storage provider 'SQLite' is healthy",
      "duration": 15.2,
      "data": {
        "provider": "SQLite",
        "responseDuration": "15.2ms",
        "message": "Storage is healthy"
      }
    },
    {
      "name": "cache",
      "status": "Healthy",
      "description": "Cache is healthy and responsive",
      "duration": 2.1,
      "data": {
        "cacheType": "MemoryCache",
        "responseDuration": "2.1ms",
        "operations": "Set, Get, Invalidate"
      }
    }
  ]
}
```

#### Health Check Components

**StorageHealthCheck** (`HealthChecks/StorageHealthCheck.cs`)
- Verifies storage provider connectivity
- Tests read/write operations
- Measures response duration
- Reports provider type (SQLite or FileSystem)
- Tagged: "ready"

**CacheHealthCheck** (`HealthChecks/CacheHealthCheck.cs`)
- Verifies memory cache functionality
- Tests write, read, and invalidate operations
- Measures cache performance
- Detects cache corruption
- Tagged: "ready"

#### Configuration

Health checks registered in `Program.cs`:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>(
        "storage",
        tags: new[] { "ready" })
    .AddCheck<CacheHealthCheck>(
        "cache",
        tags: new[] { "ready" });
```

#### Kubernetes/Container Orchestration Example

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 10
  
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 15
  periodSeconds: 5
```

### Logging
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Critical
- Correlation IDs for distributed tracing
- Sanitized logs (PII/sensitive data removed)
- Multiple sinks: Console, File, Application Insights

## Scaling Considerations

### Vertical Scaling (Scale Up)
- Increase App Service Plan tier
- Larger container resources (CPU/memory)
- Applicable to all deployment models

### Horizontal Scaling (Scale Out)
- ⚠️ **Requires sticky sessions** due to Blazor Server state
- Azure App Service: Auto-scale rules based on CPU/memory/schedule
- Kubernetes: HorizontalPodAutoscaler (HPA)
- Load balancer must support WebSocket affinity
- Shared storage required for SQLite (or use distributed cache)

### Multi-Region Deployment
- Deploy separate instances per region
- Azure Traffic Manager or Front Door for routing
- Read-replicas for configuration data (future)
- Consider data synchronization requirements

## Security Architecture

### Network Security
- HTTPS enforced via redirect middleware
- TLS 1.2+ required
- WebSocket over WSS (secure WebSocket)
- CORS configured for API access
- Content Security Policy headers

### Authentication
- API Key authentication for REST API
- Future: Azure AD integration for UI
- Managed Identity for Azure service access
- Service Principal as fallback authentication

### Data Protection
- ASP.NET Core Data Protection API
- Keys stored in persistent storage
- Encrypted at rest (SQLite encryption future enhancement)
- Audit logging for administrative actions

### Secrets Management
- Azure Key Vault (recommended for App Service)
- Docker Secrets (container orchestration)
- User Secrets (development only)
- Environment variables (avoid in production)

## Disaster Recovery

### Backup Strategy
- **Export/Import** - Built-in JSON export of all configuration
- **Storage Backup** - File system or SQLite database backup
- **Infrastructure as Code** - ARM/Bicep templates for environment recreation
- **Container Registry** - Versioned container images for rollback

### Recovery Time Objective (RTO)
- **Azure App Service**: < 15 minutes (redeploy from backup)
- **Containers**: < 5 minutes (deploy from registry)
- **Standalone**: < 30 minutes (manual restoration)

### Recovery Point Objective (RPO)
- Dependent on backup frequency
- Recommended: Daily automated backups
- Export before major configuration changes

## Consequences

### Positive

✅ **Flexibility** - Organizations can choose deployment model based on their infrastructure and expertise

✅ **Cloud Agnostic** - Core application runs on Azure, AWS, GCP, or on-premises

✅ **Containerized** - Consistent environment across platforms with Docker

✅ **Managed Options** - Azure App Service provides fully managed PaaS experience

✅ **Cost Optimization** - Range from free (self-hosted) to enterprise-scale Azure deployments

✅ **Developer Friendly** - Local development runs identically to production

### Negative

⚠️ **Complexity** - Supporting multiple deployment models requires extensive testing

⚠️ **Documentation Overhead** - Each deployment model requires separate documentation

⚠️ **WebSocket Requirements** - Blazor Server requires persistent connections (not all load balancers support this well)

⚠️ **Stateful Nature** - Blazor Server state complicates horizontal scaling

⚠️ **Storage Considerations** - SQLite on shared storage has performance implications at scale

### Mitigations

- Comprehensive deployment documentation for each model
- Docker Compose examples for common scenarios
- GitHub Actions workflows for Azure deployments
- Health checks detect configuration issues early
- Migration tools help users move between deployment models

## Alternatives Considered

### 1. Azure App Service Only
**Rejected** - Would exclude organizations without Azure subscriptions, air-gapped environments, and development scenarios.

### 2. Containers Only
**Rejected** - Higher barrier to entry for small organizations without container expertise, overkill for development/testing.

### 3. Azure Functions (Serverless)
**Rejected** - Blazor Server requires persistent connections (incompatible with stateless Functions), cold start issues for interactive UI.

### 4. Static Site Hosting (Blazor WebAssembly)
**Rejected** - Architectural decision to use Blazor Server (see ADR-001), would require significant rewrite.

## Migration Path

Users can migrate between deployment models:

1. **Export Configuration** - Use built-in export to JSON
2. **Deploy New Environment** - Set up target deployment model
3. **Import Configuration** - Restore from JSON export
4. **Validate** - Test naming generation
5. **Cutover** - Update DNS/routing to new deployment

Storage provider migration (FileSystem ↔ SQLite) supported via Admin UI.

## References

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [12-Factor App Methodology](https://12factor.net/)
- [ASP.NET Core Hosting](https://learn.microsoft.com/aspnet/core/fundamentals/host/web-host)
- [Blazor SignalR Configuration](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/signalr)

## Related ADRs

- [ADR-001: Application Architecture](ADR-001-application-architecture.md)
- [ADR-003: Data Storage Architecture](ADR-003-data-storage-architecture.md)

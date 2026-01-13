# ADR-003: Data Storage Architecture

**Status:** Accepted  
**Date:** 2025-12-09  
**Decision Makers:** Azure Naming Tool Team  
**Version:** 5.0.0

## Context

The Azure Naming Tool manages configuration data including:
- Resource types and their naming patterns
- Location mappings and abbreviations
- Organizational components (departments, environments, functions)
- Custom components and naming rules
- Admin settings and API keys
- Generated name history (optional)
- Azure Tenant validation settings

The storage solution needs to support:
- **Simple deployment** - Minimal setup for small organizations
- **High performance** - Fast reads for name generation
- **Data integrity** - Transactional consistency for configuration changes
- **Backup/restore** - Easy data portability
- **Versioning** - Support for configuration history (future)
- **Migration** - Upgrade path from existing deployments
- **Flexibility** - Different storage backends for different deployment scenarios

The tool originally used JSON file-based storage. Version 5.0.0 introduced SQLite as an alternative, with JSON remaining for backward compatibility.

## Decision

We have implemented a **dual storage provider architecture** using the Repository Pattern with two concrete implementations:

### Storage Providers

#### 1. SQLite Database (Default in v5.0.0+)

**Technology:** 
- Entity Framework Core 8.x/10.x
- SQLite 3.x
- Single-file database (`azurenamingtool.db`)

**Architecture:**
```
Repository Interfaces
         ↓
SQLiteStorageProvider (IStorageProvider)
         ↓
ConfigurationDbContext (EF Core)
         ↓
SQLite Database File
```

**Data Model:**
- Entity Framework Code-First approach
- One table per entity type (ResourceTypes, Locations, Environments, etc.)
- JSON serialization for complex properties
- Indexes on frequently queried columns
- Foreign key relationships where applicable
- Audit columns (CreatedDate, ModifiedDate) where needed

**Database Location:**
- Default: `settings/azurenamingtool.db`
- Configurable via `ConnectionStrings:DefaultConnection`
- Relative or absolute path support

**Advantages:**
- ✅ **Performance** - 10-100x faster queries than JSON file parsing
- ✅ **ACID Transactions** - Atomic, consistent, isolated, durable operations
- ✅ **Data Integrity** - Foreign key constraints, unique constraints
- ✅ **Concurrency** - Better handling of simultaneous updates
- ✅ **Querying** - Efficient filtering, sorting, pagination via LINQ
- ✅ **Scalability** - Handles larger datasets efficiently
- ✅ **Required for Azure Validation** - Azure Tenant Name Validation requires SQLite

**Considerations:**
- ⚠️ **File Locking** - Single-writer limitation (acceptable for admin-driven configuration)
- ⚠️ **Shared Storage** - Performance degradation on network file systems
- ⚠️ **Deployment Complexity** - Requires SQLite native libraries (included in .NET)
- ⚠️ **Migration Required** - Existing v4.x users must migrate from JSON

#### 2. FileSystem (JSON) Storage (Legacy, Backward Compatible)

**Technology:**
- System.Text.Json serialization
- JSON files in `settings/` directory
- One file per entity collection

**Architecture:**
```
Repository Interfaces
         ↓
FileSystemStorageProvider (IStorageProvider)
         ↓
FileSystemHelper
         ↓
JSON Files (resourcetypes.json, locations.json, etc.)
```

**File Structure:**
```
settings/
  ├── resourcetypes.json
  ├── resourcelocations.json
  ├── resourceenvironments.json
  ├── resourceorgs.json
  ├── resourceprojappsvcs.json
  ├── resourceunitdepts.json
  ├── resourcefunctions.json
  ├── resourcedelimiters.json
  ├── customcomponents.json
  ├── adminsettings.json
  ├── adminlogmessages.json
  └── generatednames.json (optional)
```

**Advantages:**
- ✅ **Simplicity** - Human-readable, easy to inspect/edit
- ✅ **No Dependencies** - No database engine required
- ✅ **Portability** - Easy copy/paste between environments
- ✅ **Version Control** - Can commit configuration to Git
- ✅ **Backward Compatible** - Existing v4.x deployments work without changes

**Considerations:**
- ⚠️ **Performance** - Full file parse on every read operation
- ⚠️ **No Transactions** - Partial update failures possible
- ⚠️ **Concurrency** - File locking issues under heavy load
- ⚠️ **Limited Validation** - No foreign key enforcement
- ⚠️ **Azure Validation Incompatible** - Cannot use Azure Tenant Name Validation feature

### Repository Pattern Implementation

**Interfaces** (`Repositories/Interfaces/`):
```csharp
public interface IStorageProvider
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value);
    Task DeleteAsync(string key);
    Task<IEnumerable<T>> GetAllAsync<T>();
}

// Entity-specific repositories
public interface IResourceTypeRepository
{
    Task<ResourceType?> GetByIdAsync(int id);
    Task<IEnumerable<ResourceType>> GetAllAsync();
    Task<ResourceType> CreateAsync(ResourceType entity);
    Task UpdateAsync(ResourceType entity);
    Task DeleteAsync(int id);
}
```

**Implementations:**
- `SQLiteStorageProvider` - Entity Framework Core operations
- `FileSystemStorageProvider` - JSON file operations
- 12+ entity-specific repositories (ResourceTypeRepository, ResourceLocationRepository, etc.)

**Configuration:**
- Storage provider selected via `appsettings.json`:
  ```json
  {
    "StorageProvider": "SQLite"  // or "FileSystem"
  }
  ```
- Runtime provider resolution via dependency injection
- Single registration ensures consistent storage across application

### Data Migration

**Built-in Migration Service** (`Services/StorageMigrationService`):

**Features:**
- One-click migration from FileSystem → SQLite (Admin UI)
- Automatic backup creation before migration
- Progress tracking and error handling
- Rollback capability if migration fails
- Validation of migrated data

**Migration Process:**
1. User initiates migration from Admin Settings page
2. System creates backup of JSON files (`settings_backup_[timestamp].zip`)
3. Reads all JSON configuration files
4. Creates SQLite database schema (EF Core migrations)
5. Imports data into SQLite tables
6. Validates data integrity (record counts, required fields)
7. Updates `appsettings.json` to use SQLite
8. Application restarts with new provider

**Rollback:**
- Restore JSON files from backup ZIP
- Update `appsettings.json` back to FileSystem
- Restart application

### Data Model

**Core Entities:**
- `ResourceType` - Azure resource types with naming patterns
- `ResourceLocation` - Azure regions and location codes
- `ResourceEnvironment` - Environment designators (prod, dev, test)
- `ResourceOrg` - Organizational units
- `ResourceProjAppSvc` - Projects/applications/services
- `ResourceUnitDept` - Business units/departments
- `ResourceFunction` - Functional designators
- `ResourceDelimiter` - Separators for name components
- `CustomComponent` - User-defined naming components
- `GeneratedName` - Name generation history (optional)
- `AdminLogMessage` - Audit log entries
- `AdminUser` - User credentials (future)

**Naming Configuration:**
- Each resource type defines pattern: `[prefix][delimiter][components][suffix]`
- Components ordered by sequence (sortOrder property)
- Enabled/disabled flags control inclusion
- Validation rules ensure pattern correctness

### Caching Strategy

**In-Memory Caching** (`Helpers/CacheHelper`):
- Configuration data cached to reduce storage reads
- Cache invalidation on configuration updates
- TTL (Time-To-Live) configurable per entity type
- Memory pressure eviction via MemoryCache
- Warm-up on application start

**Cache Keys:**
- Pattern: `{EntityType}_{Id}` or `{EntityType}_All`
- Examples: `ResourceType_1`, `ResourceLocations_All`

**Benefits:**
- Reduces SQLite queries by 90%+
- Near-instant name generation (no storage I/O)
- Handles high request volumes efficiently

**Invalidation:**
- Manual invalidation via CacheHelper.InvalidateCache()
- Automatic invalidation on Create/Update/Delete operations
- Cache cleared on storage provider change

### Configuration Export/Import

**Export Format:** JSON
- Single JSON file containing all configuration
- Schema version for compatibility checks
- Timestamp and metadata
- Human-readable formatting

**Export/Import Use Cases:**
- Backup before major changes
- Migration between environments (dev → prod)
- Disaster recovery
- Configuration sharing between instances
- Version control of naming standards

**Implementation:**
- Export: `ImportExportController.ExportConfig()` → ZIP file with all JSON
- Import: `ImportExportController.ImportConfig()` → Restore from ZIP
- Works with both SQLite and FileSystem providers
- Validates schema before import

### Data Integrity

**SQLite Constraints:**
- Primary keys on all entity IDs
- Unique constraints on names/codes
- Check constraints for valid values
- Foreign keys (where relationships exist)
- NOT NULL constraints on required fields

**Validation:**
- Model validation attributes (`[Required]`, `[MaxLength]`, `[Range]`)
- Custom validators for business rules
- Pre-save validation in repository layer
- API input validation with DataAnnotations

**Audit Trail:**
- AdminLogMessages table logs all configuration changes
- User, timestamp, action, affected entity
- Log retention configurable (default: 90 days)
- Structured logging to Application Insights

### Backup Strategy

**Automated Backups:**
- Pre-migration automatic backup
- Manual export via Admin UI
- Scheduled exports (future: Azure Blob Storage)

**Backup Contents:**
- SQLite: Single `.db` file + appsettings.json
- FileSystem: All JSON files in `settings/` directory
- Configuration export ZIP includes all data

**Retention:**
- User-managed (no automatic cleanup)
- Recommended: Daily backups retained for 30 days

**Restore Process:**
1. Stop application
2. Replace database/JSON files with backup
3. Restart application
4. Validate configuration loads correctly

## Consequences

### Positive

✅ **Performance Improvement** - SQLite provides 10-100x faster queries than JSON parsing

✅ **Data Integrity** - ACID transactions prevent partial updates and data corruption

✅ **Backward Compatibility** - FileSystem provider maintains v4.x compatibility

✅ **Flexibility** - Organizations choose storage based on their needs

✅ **Migration Path** - Built-in tool makes upgrading from v4.x seamless

✅ **Future-Proof** - Repository pattern allows adding new providers (Azure SQL, Cosmos DB)

✅ **Developer Experience** - Entity Framework provides strong typing and LINQ queries

✅ **Required for Azure Validation** - SQLite enables the flagship v5.0.0 feature

### Negative

⚠️ **Increased Complexity** - Dual provider support requires extensive testing

⚠️ **Migration Overhead** - Users must migrate to access Azure Tenant Name Validation

⚠️ **SQLite Limitations** - Not suitable for high-concurrency write scenarios (acceptable for admin-driven config)

⚠️ **Deployment Consideration** - SQLite on network storage (NFS/SMB) has poor performance

⚠️ **Documentation Burden** - Need to document both providers and migration process

### Mitigations

- Migration tool with automatic backup reduces migration risk
- Clear documentation explaining when to use each provider
- Health checks detect storage issues early
- Caching layer reduces impact of storage performance
- Fallback to FileSystem if SQLite issues occur

## Performance Characteristics

### SQLite Provider
- **Read Operation**: < 1ms (with indexes)
- **Write Operation**: < 5ms (single record)
- **Query with Filter**: < 2ms (indexed columns)
- **Full Configuration Load**: < 50ms (cached after first load)
- **Concurrency**: Single writer, multiple readers

### FileSystem Provider
- **Read Operation**: 5-20ms (full file parse)
- **Write Operation**: 10-50ms (serialize + write)
- **Query with Filter**: 20-100ms (load + LINQ)
- **Full Configuration Load**: 100-500ms (all files)
- **Concurrency**: File locking, retries on conflicts

### Caching Layer
- **Cache Hit**: < 0.1ms
- **Cache Miss**: Falls back to storage provider
- **Cache Invalidation**: < 1ms
- **Memory Usage**: ~5-10 MB for typical configuration

## Scalability Considerations

### SQLite Scaling
- **Vertical Scaling**: Increase disk I/O, use SSD
- **Horizontal Scaling**: Read replicas possible (requires WAL mode)
- **Limitations**: Single database file, shared storage performance issues
- **Maximum Size**: Practically unlimited (tested to 281 TB, typical usage < 100 MB)

### FileSystem Scaling
- **Vertical Scaling**: Faster disk, more memory for caching
- **Horizontal Scaling**: Read-only replicas possible
- **Limitations**: File locking, no transactions, poor concurrency

### Future Scalability Options
- Azure SQL Database (multi-region, high availability)
- Cosmos DB (global distribution, unlimited scale)
- Redis (distributed cache with persistence)
- PostgreSQL (open source, advanced features)

## Security Considerations

### Data at Rest
- SQLite: File system encryption (BitLocker, Azure Disk Encryption)
- FileSystem: File system encryption
- Future: SQLite encryption extension (SEE)

### Data in Transit
- All API access over HTTPS
- Internal storage access is local file system (no network transmission)

### Access Control
- API key authentication for API endpoints
- Admin authentication for configuration changes (future: Azure AD)
- File system permissions restrict access to `settings/` directory

### Secrets Management
- API keys stored in AdminSettings (encrypted in future)
- Azure Service Principal credentials encrypted
- Sensitive data sanitized from logs

## Alternatives Considered

### 1. SQL Server / Azure SQL Database
**Rejected** - Too heavy for simple deployments, requires separate database instance, licensing costs, overkill for configuration storage.

### 2. Cosmos DB
**Rejected** - Excellent for global scale but unnecessary complexity and cost for configuration data, no local development option without emulator.

### 3. JSON Only (No SQLite)
**Rejected** - Would limit Azure Tenant Name Validation feature, poor performance at scale, no transaction support.

### 4. SQLite Only (No JSON)
**Rejected** - Would break backward compatibility with v4.x deployments, removes simple text-based configuration option.

### 5. Redis
**Rejected** - Adds external dependency, requires separate Redis instance, complexity not justified for configuration storage.

### 6. In-Memory Database Only
**Rejected** - No persistence, configuration lost on restart, unacceptable for production use.

## Migration from v4.x to v5.0.0

**Automatic Detection:**
- v5.0.0 detects FileSystem configuration on startup
- Admin UI shows "Migrate to SQLite" option
- Migration is optional (FileSystem continues to work)

**User Journey:**
1. User upgrades to v5.0.0 (FileSystem provider continues working)
2. User navigates to Admin → Storage Provider Settings
3. User clicks "Migrate to SQLite"
4. System creates backup automatically
5. Migration completes with progress indicators
6. User confirms successful migration
7. Azure Tenant Name Validation feature becomes available

**Rollback:**
1. Stop application
2. Restore JSON files from backup ZIP
3. Edit `appsettings.json`: Set `StorageProvider` to `FileSystem`
4. Restart application

## Future Enhancements

**Planned:**
- ✅ Azure SQL Database provider (high availability, multi-region)
- ✅ Configuration versioning (track changes over time)
- ✅ Audit log retention policies (automatic cleanup)
- ✅ SQLite Write-Ahead Logging (WAL) mode (better concurrency)
- ✅ Backup to Azure Blob Storage (automated backups)
- ✅ Import/Export via API (automation scenarios)

**Under Consideration:**
- PostgreSQL provider (open source, advanced features)
- Read replicas for high-read scenarios
- Event sourcing for configuration changes
- Real-time synchronization across instances
- GraphQL API for flexible queries

## References

- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [Entity Framework Core Documentation](https://learn.microsoft.com/ef/core/)
- [Repository Pattern](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [System.Text.Json Documentation](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json-overview)
- [SQLite Performance Tuning](https://www.sqlite.org/fasterthanfs.html)

## Related ADRs

- [ADR-001: Application Architecture](ADR-001-application-architecture.md)
- [ADR-002: Hosting Architecture](ADR-002-hosting-architecture.md)

# API Bulk Operation Implementation Plan

## Overview

This document outlines the implementation plan for adding bulk API operations to the Azure Naming Tool. The primary use case is to allow users to generate multiple resource names using the same resource component data across different resource types in a single API call.

**Important**: Bulk operations will **ONLY** be added to the V2 API. The V1 (original) API will remain unchanged and will not include any bulk operation endpoints. This maintains backward compatibility and keeps the V1 API stable for existing integrations.

## Business Requirements

### Primary Use Case
Users need to generate names for multiple resource types that share the same resource components (e.g., environment, location, org, etc.). For example, when deploying a new application, they might need names for:
- Storage Account
- Key Vault
- App Service
- SQL Database
- Application Insights

All using the same environment (prod), location (eastus), org (finance), etc.

### Key Requirements
1. **Resilience**: If one resource type name generation fails, continue processing the remaining items
2. **Detailed Feedback**: Return comprehensive results showing which operations succeeded and which failed
3. **Performance**: Process multiple requests efficiently
4. **Compatibility**: Maintain backward compatibility with existing single-item endpoints
5. **Validation**: Validate all inputs before processing
6. **Extensibility**: Design should support future bulk operations beyond name generation

## Technical Design

### 1. API Endpoint Design

#### Endpoint Pattern
Bulk operations are **exclusively available in the V2 API**:

```
POST /api/v2/ResourceNamingRequests/GenerateBulk
POST /api/v2/ResourceComponents/UpdateBulk (future)
POST /api/v2/ResourceTypes/UpdateBulk (future)
```

**Important Notes**:
- ❌ **NO bulk endpoints in V1 API** (`/api/v1/*`)
- ✅ **V2 API only** (`/api/v2/*`)
- V1 API endpoints remain unchanged and unaffected
- Existing V1 integrations continue to work without modification

**Rationale**: 
- Clearer intent than generic batch endpoint
- Easier to version and maintain
- Better API documentation
- Simpler authorization/rate limiting per operation type
- Keeps V1 API stable for legacy integrations

### 2. Request/Response Models

#### Bulk Name Generation Request Model

```csharp
public class BulkResourceNameRequest
{
    /// <summary>
    /// List of resource type IDs or short names to generate names for
    /// </summary>
    public List<string> ResourceTypes { get; set; } = new();
    
    /// <summary>
    /// Common resource components used for all resource types
    /// Can be resource component IDs or values
    /// </summary>
    public Dictionary<string, string> ResourceComponents { get; set; } = new();
    
    /// <summary>
    /// Optional: Resource type specific overrides
    /// Key = ResourceType identifier, Value = component overrides
    /// </summary>
    public Dictionary<string, Dictionary<string, string>>? ResourceTypeOverrides { get; set; }
    
    /// <summary>
    /// Whether to continue processing if individual items fail (default: true)
    /// </summary>
    public bool ContinueOnError { get; set; } = true;
    
    /// <summary>
    /// Optional: Validate names only without generating (default: false)
    /// </summary>
    public bool ValidateOnly { get; set; } = false;
    
    /// <summary>
    /// Optional: Include detailed validation messages in response
    /// </summary>
    public bool IncludeValidationDetails { get; set; } = true;
}
```

**Example Request JSON**:
```json
{
  "resourceTypes": ["sa", "kv", "app", "sqldb", "appi"],
  "resourceComponents": {
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus",
    "resourceOrg": "finance",
    "resourceProjAppSvc": "payroll",
    "resourceUnitDept": "hr"
  },
  "resourceTypeOverrides": {
    "kv": {
      "resourceLocation": "westus"
    }
  },
  "continueOnError": true,
  "validateOnly": false,
  "includeValidationDetails": true
}
```

#### Bulk Name Generation Response Model

```csharp
public class BulkResourceNameResponse
{
    /// <summary>
    /// Overall status of the bulk operation
    /// </summary>
    public BulkOperationStatus Status { get; set; }
    
    /// <summary>
    /// Total number of resource types requested
    /// </summary>
    public int TotalRequested { get; set; }
    
    /// <summary>
    /// Number of successful name generations
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed name generations
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Individual results for each resource type
    /// </summary>
    public List<BulkResourceNameResult> Results { get; set; } = new();
    
    /// <summary>
    /// Overall error message if the entire operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp when the operation completed
    /// </summary>
    public DateTime CompletedAt { get; set; }
    
    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long DurationMs { get; set; }
}

public class BulkResourceNameResult
{
    /// <summary>
    /// Resource type identifier (short name or ID)
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Success status for this specific resource type
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Generated resource name (if successful)
    /// </summary>
    public string? ResourceName { get; set; }
    
    /// <summary>
    /// Error message (if failed)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Validation messages and warnings
    /// </summary>
    public List<ValidationMessage>? ValidationMessages { get; set; }
    
    /// <summary>
    /// Components used to generate this name
    /// </summary>
    public Dictionary<string, string>? ComponentsUsed { get; set; }
    
    /// <summary>
    /// Full resource type details
    /// </summary>
    public ResourceTypeDetails? ResourceTypeInfo { get; set; }
}

public class ValidationMessage
{
    public string Severity { get; set; } = "Info"; // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
}

public class ResourceTypeDetails
{
    public int Id { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public enum BulkOperationStatus
{
    Success,           // All operations succeeded
    PartialSuccess,    // Some succeeded, some failed
    Failed,            // All operations failed
    ValidationError    // Request validation failed before processing
}
```

**Example Response JSON**:
```json
{
  "status": "PartialSuccess",
  "totalRequested": 5,
  "successCount": 4,
  "failureCount": 1,
  "results": [
    {
      "resourceType": "sa",
      "success": true,
      "resourceName": "saprodeastusfinancepayrollhr",
      "errorMessage": null,
      "validationMessages": [],
      "componentsUsed": {
        "resourceEnvironment": "prod",
        "resourceLocation": "eastus",
        "resourceOrg": "finance",
        "resourceProjAppSvc": "payroll",
        "resourceUnitDept": "hr"
      },
      "resourceTypeInfo": {
        "id": 245,
        "shortName": "sa",
        "resource": "Storage/storageAccounts",
        "scope": "global"
      }
    },
    {
      "resourceType": "kv",
      "success": true,
      "resourceName": "kv-prod-westus-finance-payroll-hr",
      "errorMessage": null,
      "validationMessages": [
        {
          "severity": "Info",
          "message": "Used override location: westus",
          "field": "resourceLocation"
        }
      ],
      "componentsUsed": {
        "resourceEnvironment": "prod",
        "resourceLocation": "westus",
        "resourceOrg": "finance",
        "resourceProjAppSvc": "payroll",
        "resourceUnitDept": "hr"
      },
      "resourceTypeInfo": {
        "id": 154,
        "shortName": "kv",
        "resource": "KeyVault/vaults",
        "scope": "global"
      }
    },
    {
      "resourceType": "sqldb",
      "success": false,
      "resourceName": null,
      "errorMessage": "Name exceeds maximum length of 128 characters",
      "validationMessages": [
        {
          "severity": "Error",
          "message": "Generated name length (135) exceeds maximum allowed (128)",
          "field": "lengthMax"
        }
      ],
      "componentsUsed": null,
      "resourceTypeInfo": {
        "id": 267,
        "shortName": "sqldb",
        "resource": "Sql/servers/databases",
        "scope": "server"
      }
    }
  ],
  "errorMessage": null,
  "completedAt": "2025-10-24T14:30:45.123Z",
  "durationMs": 234
}
```

### 3. Controller Implementation

#### New Controller: `ResourceNamingRequestsController` (V2 API)

```csharp
[ApiController]
[Route("api/v2/[controller]")]
public class ResourceNamingRequestsController : ControllerBase
{
    private readonly ILogger<ResourceNamingRequestsController> _logger;
    private readonly IResourceNameGenerationService _nameGenerationService;
    
    [HttpPost("GenerateBulk")]
    [ProducesResponseType(typeof(BulkResourceNameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BulkResourceNameResponse), StatusCodes.Status207MultiStatus)]
    [ProducesResponseType(typeof(BulkResourceNameResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateBulk([FromBody] BulkResourceNameRequest request)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Input validation
        if (request.ResourceTypes == null || !request.ResourceTypes.Any())
        {
            return BadRequest(new BulkResourceNameResponse
            {
                Status = BulkOperationStatus.ValidationError,
                ErrorMessage = "ResourceTypes array cannot be empty",
                CompletedAt = DateTime.UtcNow,
                DurationMs = 0
            });
        }
        
        var response = await _nameGenerationService.GenerateBulkNamesAsync(request);
        
        stopwatch.Stop();
        response.CompletedAt = DateTime.UtcNow;
        response.DurationMs = stopwatch.ElapsedMilliseconds;
        
        // Return appropriate status code
        return response.Status switch
        {
            BulkOperationStatus.Success => Ok(response),
            BulkOperationStatus.PartialSuccess => StatusCode(StatusCodes.Status207MultiStatus, response),
            BulkOperationStatus.ValidationError => BadRequest(response),
            BulkOperationStatus.Failed => StatusCode(StatusCodes.Status207MultiStatus, response),
            _ => Ok(response)
        };
    }
}
```

### 4. Service Layer Implementation

#### Interface: `IResourceNameGenerationService`

```csharp
public interface IResourceNameGenerationService
{
    Task<BulkResourceNameResponse> GenerateBulkNamesAsync(BulkResourceNameRequest request);
    Task<BulkResourceNameResult> GenerateSingleNameAsync(
        string resourceType, 
        Dictionary<string, string> components);
}
```

#### Implementation Strategy

```csharp
public class ResourceNameGenerationService : IResourceNameGenerationService
{
    public async Task<BulkResourceNameResponse> GenerateBulkNamesAsync(
        BulkResourceNameRequest request)
    {
        var response = new BulkResourceNameResponse
        {
            TotalRequested = request.ResourceTypes.Count,
            Results = new List<BulkResourceNameResult>()
        };
        
        foreach (var resourceType in request.ResourceTypes)
        {
            try
            {
                // Merge common components with type-specific overrides
                var components = MergeComponents(
                    request.ResourceComponents,
                    request.ResourceTypeOverrides?.GetValueOrDefault(resourceType)
                );
                
                // Generate name for this resource type
                var result = await GenerateSingleNameAsync(resourceType, components);
                
                response.Results.Add(result);
                
                if (result.Success)
                    response.SuccessCount++;
                else
                    response.FailureCount++;
            }
            catch (Exception ex)
            {
                // Log error but continue if ContinueOnError is true
                _logger.LogError(ex, "Failed to generate name for resource type: {ResourceType}", 
                    resourceType);
                
                response.Results.Add(new BulkResourceNameResult
                {
                    ResourceType = resourceType,
                    Success = false,
                    ErrorMessage = ex.Message
                });
                response.FailureCount++;
                
                if (!request.ContinueOnError)
                    break;
            }
        }
        
        // Determine overall status
        response.Status = DetermineStatus(response);
        
        return response;
    }
    
    private BulkOperationStatus DetermineStatus(BulkResourceNameResponse response)
    {
        if (response.FailureCount == 0)
            return BulkOperationStatus.Success;
        if (response.SuccessCount == 0)
            return BulkOperationStatus.Failed;
        return BulkOperationStatus.PartialSuccess;
    }
    
    private Dictionary<string, string> MergeComponents(
        Dictionary<string, string> baseComponents,
        Dictionary<string, string>? overrides)
    {
        if (overrides == null || !overrides.Any())
            return new Dictionary<string, string>(baseComponents);
            
        var merged = new Dictionary<string, string>(baseComponents);
        foreach (var kvp in overrides)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }
}
```

### 5. Performance Considerations

#### Parallel Processing (Optional Enhancement)

For large bulk requests, consider parallel processing:

```csharp
public async Task<BulkResourceNameResponse> GenerateBulkNamesAsync(
    BulkResourceNameRequest request)
{
    var response = new BulkResourceNameResponse
    {
        TotalRequested = request.ResourceTypes.Count,
        Results = new List<BulkResourceNameResult>()
    };
    
    // Process in parallel with max degree of parallelism
    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };
    
    var results = new ConcurrentBag<BulkResourceNameResult>();
    
    await Parallel.ForEachAsync(request.ResourceTypes, options, async (resourceType, ct) =>
    {
        try
        {
            var components = MergeComponents(
                request.ResourceComponents,
                request.ResourceTypeOverrides?.GetValueOrDefault(resourceType)
            );
            
            var result = await GenerateSingleNameAsync(resourceType, components);
            results.Add(result);
        }
        catch (Exception ex)
        {
            results.Add(new BulkResourceNameResult
            {
                ResourceType = resourceType,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    });
    
    response.Results = results.OrderBy(r => r.ResourceType).ToList();
    response.SuccessCount = results.Count(r => r.Success);
    response.FailureCount = results.Count(r => !r.Success);
    response.Status = DetermineStatus(response);
    
    return response;
}
```

**Note**: Only use parallel processing if thread-safety is ensured in all dependencies.

### 6. Rate Limiting & Throttling

Add rate limiting to prevent abuse:

```csharp
[ApiController]
[Route("api/v2/[controller]")]
[RateLimit(Name = "BulkOperations", Seconds = 60, Limit = 10)] // Max 10 bulk requests per minute
public class ResourceNamingRequestsController : ControllerBase
{
    [HttpPost("GenerateBulk")]
    [RateLimit(Name = "GenerateBulk", Seconds = 60, Limit = 5)] // Max 5 per minute per endpoint
    public async Task<IActionResult> GenerateBulk([FromBody] BulkResourceNameRequest request)
    {
        // Limit max batch size
        if (request.ResourceTypes.Count > 50)
        {
            return BadRequest(new BulkResourceNameResponse
            {
                Status = BulkOperationStatus.ValidationError,
                ErrorMessage = "Maximum 50 resource types per bulk request"
            });
        }
        
        // ... implementation
    }
}
```

## Implementation Phases

### Phase 1: Core Bulk Name Generation (MVP)
**Timeline**: 2-3 weeks

1. **Week 1**: Models & Infrastructure
   - Create request/response models
   - Add new controller endpoint
   - Implement basic service layer
   - Add input validation

2. **Week 2**: Core Logic & Testing
   - Implement name generation logic
   - Add error handling
   - Write unit tests
   - Integration tests

3. **Week 3**: Documentation & Polish
   - API documentation (Swagger)
   - User documentation
   - Performance testing
   - Code review & refinement

**Deliverables**:
- ✅ POST /api/v2/ResourceNamingRequests/GenerateBulk endpoint
- ✅ Support for multiple resource types with shared components
- ✅ Detailed success/failure reporting
- ✅ Continue-on-error functionality
- ✅ API documentation

### Phase 2: Enhanced Features
**Timeline**: 1-2 weeks

1. **Performance Optimization**
   - Parallel processing for large batches
   - Caching optimizations
   - Database query optimization

2. **Advanced Validation**
   - Validate-only mode
   - Detailed validation messages
   - Cross-resource type validation

3. **Resource Type Overrides**
   - Per-type component overrides
   - Override validation

**Deliverables**:
- ✅ Parallel processing support
- ✅ ValidateOnly mode
- ✅ Resource type specific overrides
- ✅ Enhanced validation feedback

### Phase 3: Additional Bulk Operations
**Timeline**: 2-3 weeks

Extend bulk operations to other endpoints:

1. **Bulk Component Updates**
   - POST /api/v2/ResourceEnvironments/UpdateBulk
   - POST /api/v2/ResourceLocations/UpdateBulk
   - POST /api/v2/ResourceOrgs/UpdateBulk

2. **Bulk Resource Type Updates**
   - POST /api/v2/ResourceTypes/UpdateBulk

3. **Bulk Validation**
   - POST /api/v2/ResourceNames/ValidateBulk

**Deliverables**:
- ✅ Additional bulk endpoints
- ✅ Consistent API patterns
- ✅ Comprehensive documentation

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class BulkResourceNameGenerationTests
{
    [TestMethod]
    public async Task GenerateBulk_AllSucceed_ReturnsSuccess()
    {
        // Test all resource types succeed
    }
    
    [TestMethod]
    public async Task GenerateBulk_PartialFailure_ReturnsPartialSuccess()
    {
        // Test some succeed, some fail
    }
    
    [TestMethod]
    public async Task GenerateBulk_AllFail_ReturnsFailed()
    {
        // Test all resource types fail
    }
    
    [TestMethod]
    public async Task GenerateBulk_WithOverrides_AppliesCorrectly()
    {
        // Test resource type specific overrides
    }
    
    [TestMethod]
    public async Task GenerateBulk_ContinueOnError_ContinuesAfterFailure()
    {
        // Test continue on error flag
    }
    
    [TestMethod]
    public async Task GenerateBulk_EmptyRequest_ReturnsValidationError()
    {
        // Test empty resource types array
    }
}
```

### Integration Tests
- Test with real database/storage
- Test with various resource type combinations
- Test performance with large batches (50+ items)
- Test concurrent bulk requests

### Performance Tests
- Benchmark single vs bulk operations
- Test memory usage with large requests
- Test parallel vs sequential processing
- Stress test with multiple concurrent users

## API Documentation

### Swagger/OpenAPI Example

```yaml
/api/v2/ResourceNamingRequests/GenerateBulk:
  post:
    summary: Generate multiple resource names in a single request
    description: |
      Generates names for multiple resource types using shared resource components.
      If a resource type fails, processing continues for remaining types by default.
    tags:
      - Resource Naming Requests
    requestBody:
      required: true
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/BulkResourceNameRequest'
          examples:
            basicExample:
              summary: Basic bulk generation
              value:
                resourceTypes: ["sa", "kv", "app"]
                resourceComponents:
                  resourceEnvironment: "prod"
                  resourceLocation: "eastus"
                continueOnError: true
            withOverrides:
              summary: With resource type overrides
              value:
                resourceTypes: ["sa", "kv", "sqldb"]
                resourceComponents:
                  resourceEnvironment: "prod"
                  resourceLocation: "eastus"
                resourceTypeOverrides:
                  kv:
                    resourceLocation: "westus"
    responses:
      200:
        description: All resource names generated successfully
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BulkResourceNameResponse'
      207:
        description: Partial success - some succeeded, some failed
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BulkResourceNameResponse'
      400:
        description: Validation error in request
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BulkResourceNameResponse'
```

## Security Considerations

1. **Rate Limiting**: Prevent DoS attacks with bulk requests
2. **Input Validation**: Sanitize all inputs, limit batch sizes
3. **Authentication**: Same auth requirements as single operations
4. **Audit Logging**: Log bulk operations for compliance
5. **Resource Quotas**: Limit concurrent bulk operations per user

## Migration & Backward Compatibility

- Existing single-item endpoints remain unchanged
- V2 API introduced for bulk operations
- V1 API continues to work for existing integrations
- No breaking changes to existing functionality

## Future Enhancements

1. **Async/Background Processing**
   - For very large bulk operations (100+ items)
   - Return job ID, poll for status
   - Webhook notifications on completion

2. **Batch Import from CSV/JSON**
   - Upload file with multiple requests
   - Generate names for entire infrastructure

3. **Naming Templates**
   - Save bulk request as reusable template
   - Apply template across multiple projects

4. **Bulk Export**
   - Export generated names to CSV/JSON
   - Integration with Infrastructure-as-Code tools

## Success Metrics

- **Performance**: Bulk operation 5-10x faster than sequential individual calls
- **Adoption**: 30% of API users adopt bulk operations within 3 months
- **Reliability**: 99.9% success rate for valid bulk requests
- **User Satisfaction**: Positive feedback from beta users

## Conclusion

This implementation plan provides a robust, scalable solution for bulk API operations in the Azure Naming Tool. The design prioritizes:

✅ **Resilience**: Continue processing on individual failures  
✅ **Transparency**: Detailed success/failure reporting  
✅ **Performance**: Efficient batch processing  
✅ **Extensibility**: Easy to add new bulk operation types  
✅ **Compatibility**: No breaking changes to existing API

The phased approach allows for incremental delivery of value while maintaining quality and stability.

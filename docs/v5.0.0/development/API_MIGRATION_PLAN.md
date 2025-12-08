# Azure Naming Tool API Migration Plan

## Executive Summary

This document outlines a comprehensive plan to modernize the Azure Naming Tool REST API while maintaining **TRUE backward compatibility** for existing users. The current API is functional but lacks several modern features and best practices that would improve developer experience, security, and maintainability.

### 🎯 **Versioning Strategy: v1 (Legacy) vs v2 (Modern)**

**CRITICAL:** To ensure true backward compatibility, we maintain two separate API versions:

- **v1 (Legacy)**: `/api/[controller]` - Maintains original behavior including:
  - Always returns `200 OK` status (even for errors)
  - Returns plain strings with `"SUCCESS"` or `"FAILURE - ..."` messages
  - No structural changes to existing endpoints
  - **Guarantees existing clients continue to work without modification**

- **v2 (Modern)**: `/api/v2/[controller]` - Modern REST best practices:
  - Proper HTTP status codes (200, 400, 401, 404, 500)
  - Standardized `ApiResponse<T>` wrapper with error codes
  - Correlation IDs included in responses
  - Structured error messages with error codes
  - Better error handling and diagnostics

**Why This Matters:**
- Users can upgrade to new versions without breaking their existing integrations
- Migration to v2 is optional and can be done at each user's pace
- No forced breaking changes on upgrade
- Best practice for API versioning (Netflix, GitHub, Stripe model)

## ✅ Completed Work

### Phase 1: Stabilization & Documentation ✅ COMPLETE

**✅ Enhanced Swagger Documentation**
- Added `[Produces("application/json")]` to all 14 controllers
- Added `[ProducesResponseType]` attributes to key endpoints (ResourceNamingRequests, ResourceTypes, Admin)
- Documents HTTP status codes in Swagger UI

**✅ Correlation ID Middleware**
- Created `CorrelationIdMiddleware` for request tracking
- Automatic `X-Correlation-ID` header on all responses
- Supports client-provided correlation IDs
- Enables better debugging and log correlation

**✅ API Logging Middleware**
- Created `ApiLoggingMiddleware` for audit trail
- Structured logging with correlation IDs
- Logs request method, path, API key (masked), body (POST/PUT)
- Logs response status, duration, body for errors
- Performance warnings for slow requests (>5s)

**✅ Standardized API Response Models**
- Created `ApiResponse<T>` generic wrapper
- Created `ApiError` class following Microsoft REST API Guidelines
- Created `ApiMetadata` with correlation ID, timestamp, version
- Created `ApiInnerError` for detailed debugging
- Created `PaginationMetadata` for future list endpoints
- **Note:** These models are used in v2 only for backward compatibility

**✅ XML Documentation**
- Added XML documentation to all middleware classes
- Build completes with 0 warnings

### Phase 2: API Versioning ✅ COMPLETE

**✅ Installed API Versioning Package**
- Installed `Asp.Versioning.Mvc.ApiExplorer` v8.1.0

**✅ Configured API Versioning**
- URL-based versioning: `/api/v1/[controller]`, `/api/v2/[controller]`
- Header-based versioning: `X-Api-Version` header support
- Default version: 1.0 (backward compatible)
- Reports supported versions in response headers (`api-supported-versions`)

**✅ Updated Swagger Configuration**
- Separate documentation for v1 and v2
- Version dropdown in Swagger UI
- DisplayRequestDuration and EnableTryItOutByDefault

**✅ Added Version Attributes to v1 Controllers**
- All 14 existing controllers marked as `[ApiVersion("1.0")]`
- v1 maintains legacy behavior (200 OK with "SUCCESS"/"FAILURE" strings)

**✅ Created v2 Controllers with Modern Practices**

**Phase 2A: COMPLETE ✅** - All 14 controllers now have V2 versions:

1. ✅ `V2/AdminController` - Admin operations
   - Password management with 401 Unauthorized for incorrect password
   - 400 BadRequest for missing/invalid inputs
   - Uses `ApiResponse<T>` wrapper with correlation IDs

2. ✅ `V2/ResourceTypesController` - Resource type management
   - 404 NotFound for missing resources
   - Proper HTTP status codes
   - Structured error responses

3. ✅ `V2/ResourceNamingRequestsController` - Name generation (MOST IMPORTANT)
   - RequestName endpoint (RECOMMENDED)
   - RequestNameWithComponents endpoint
   - ValidateName endpoint
   - Enhanced error handling with validation details

4. ✅ `V2/ResourceDelimitersController` - Delimiter management
5. ✅ `V2/ResourceEnvironmentsController` - Environment management with DELETE support
6. ✅ `V2/ResourceFunctionsController` - Function management with DELETE support
7. ✅ `V2/ResourceLocationsController` - Location management
8. ✅ `V2/ResourceOrgsController` - Organization management with DELETE support
9. ✅ `V2/ResourceProjAppSvcsController` - Project/App/Service management with DELETE support
10. ✅ `V2/ResourceUnitDeptsController` - Unit/Department management with DELETE support
11. ✅ `V2/ResourceComponentsController` - Component management
12. ✅ `V2/CustomComponentsController` - Custom component management with DELETE support
13. ✅ `V2/PolicyController` - Azure Policy definition generation
14. ✅ `V2/ImportExportController` - Configuration import/export

**Common V2 Features:**
- Uses `ApiResponse<T>` wrapper for consistent response format
- Proper HTTP status codes (200 OK, 400 BadRequest, 404 NotFound, 500 InternalServerError)
- Correlation IDs in all responses (via `HttpContext.TraceIdentifier`)
- Structured error responses with error codes (`FETCH_FAILED`, `NOT_FOUND`, `UPDATE_FAILED`, etc.)
- Inner error details for debugging (`ApiInnerError` with exception type)
- Null request validation
- Enhanced exception handling with `ApiResponse<object>.ErrorResponse()`
- DELETE endpoint support where applicable
- Route pattern: `/api/v2/[Controller]/[Action]`

**✅ True Backward Compatibility**
- `/api/Admin/UpdatePassword` (v1) - Returns 200 OK with "SUCCESS"/"FAILURE"
- `/api/v1/Admin/UpdatePassword` (v1) - Same as above
- `/api/v2/Admin/UpdatePassword` (v2) - Returns 200/400/401 with `ApiResponse<T>`
- All v1 endpoints continue to work unchanged
- Clients can migrate to v2 at their own pace

## Current State Assessment

### What We Have (Strengths)

✅ **Functional REST API**
- Well-structured controllers with clear endpoints
- Swagger/OpenAPI documentation enabled
- Dependency injection implemented
- Service layer architecture
- Clear separation of concerns

✅ **Security Features**
- Three-tier API key system (Full Access, Name Generation, Read-Only)
- API key validation via custom attribute (`[ApiKey]`)
- Encrypted API keys in configuration
- HTTPS support

✅ **Developer Experience**
- XML documentation comments on most endpoints
- Swagger UI at `/swagger`
- Consistent response patterns

### What's Missing (Remaining Work)

❌ **Security & Performance**
- No rate limiting implementation (basic RateFilter exists but not applied)
- No request throttling
- No CORS policy configuration visible
- No request size limits enforced

❌ **Observability**
- No OpenTelemetry/Application Insights integration
- No performance metrics dashboard

❌ **Developer Experience**
- No client SDK generation
- No Postman collection published
- Limited API testing examples
- No API changelog

❌ **Modern Patterns**
- No bulk operation endpoints
- No webhook support for async operations
- No GraphQL alternative (optional)

❌ **Modern Patterns**
- Not using ASP.NET Core Minimal APIs
- No GraphQL alternative
- No webhook support for async operations
- No bulk operation endpoints
- No HATEOAS links

## Migration Strategy

### Phase 1: Stabilization & Documentation (No Breaking Changes)

**Goal**: Improve current API without breaking existing integrations

#### 1.1 Enhanced Response Standardization
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public ApiMetadata Metadata { get; set; }
}

public class ApiError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string? Target { get; set; }
    public List<ApiError>? Details { get; set; }
}

public class ApiMetadata
{
    public string RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Version { get; set; }
}
```

**Action Items**:
- [ ] Create standardized response models
- [ ] Create result filter to wrap responses
- [ ] Add correlation ID middleware
- [ ] Update all controllers to return consistent status codes
- [ ] Maintain backward compatibility by keeping existing response format as option

#### 1.2 Add Response Type Attributes
```csharp
[HttpGet("{id}")]
[ProducesResponseType(typeof(ResourceType), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Get(int id)
```

**Action Items**:
- [ ] Add `[ProducesResponseType]` to all endpoints
- [ ] Add `[Produces("application/json")]` to controllers
- [ ] Document all possible HTTP status codes
- [ ] Regenerate Swagger documentation

#### 1.3 Implement Rate Limiting
```csharp
// Use ASP.NET Core 8 built-in rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var apiKey = context.Request.Headers["APIKey"].ToString();
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromHours(1)
            });
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new ApiError
        {
            Code = "RateLimitExceeded",
            Message = "Too many requests. Please try again later."
        });
    };
});
```

**Action Items**:
- [ ] Implement ASP.NET Core 8 rate limiting
- [ ] Configure different limits per API key type:
  - Full Access: 10,000/hour
  - Name Generation: 5,000/hour  
  - Read-Only: 2,000/hour
- [ ] Add rate limit headers to responses (`X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`)
- [ ] Document rate limits in API documentation

#### 1.4 Enhanced Logging & Observability
```csharp
// Add correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// Add request/response logging
app.UseMiddleware<ApiLoggingMiddleware>();

// Structured logging
_logger.LogInformation(
    "API request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
    context.Request.Method,
    context.Request.Path,
    context.Response.StatusCode,
    stopwatch.ElapsedMilliseconds
);
```

**Action Items**:
- [ ] Create correlation ID middleware
- [ ] Create API logging middleware (log requests/responses)
- [ ] Add structured logging throughout controllers
- [ ] Add Application Insights integration (optional)
- [ ] Create API metrics dashboard configuration

#### 1.5 API Documentation Improvements
**Action Items**:
- [ ] Enhance Swagger configuration with better descriptions
- [ ] Add example requests/responses to XML comments
- [ ] Create comprehensive API documentation in `/docs/API_REFERENCE.md`
- [ ] Generate and publish Postman collection
- [ ] Create API quickstart guide
- [ ] Add code samples for common languages (C#, PowerShell, Python, JavaScript)

### Phase 2: API Versioning (Backward Compatible)

**Goal**: Introduce versioning system without breaking existing clients

#### 2.1 Implement URL-Based Versioning
```csharp
// Current (v1 - legacy, no version in URL)
// /api/ResourceTypes

// New versioned approach
// /api/v1/ResourceTypes  (redirects to legacy)
// /api/v2/ResourceTypes  (new features)
```

**Strategy**:
- Keep existing `/api/ResourceTypes` working (treat as v1)
- Add `/api/v1/ResourceTypes` that points to same implementation
- Create `/api/v2/` namespace for new features
- Use `Microsoft.AspNetCore.Mvc.Versioning` package

**Action Items**:
- [ ] Install `Asp.Versioning.Mvc.ApiExplorer` NuGet package
- [ ] Configure API versioning in `Program.cs`
- [ ] Create v1 controllers (duplicate current controllers)
- [ ] Update Swagger to show multiple API versions
- [ ] Create deprecation policy document
- [ ] Add API version headers to responses

#### 2.2 Version-Specific Improvements (V2)
**New features for v2**:
- Standardized `ApiResponse<T>` wrapper
- HATEOAS links for resource navigation
- Pagination for list endpoints
- Filtering and sorting query parameters
- Bulk operations
- Async operation support with webhooks

### Phase 3: Modern API Features (New Capabilities)

**Goal**: Add modern API features alongside existing REST API

#### 3.1 GraphQL API (Additive)
```csharp
// Add GraphQL endpoint: /graphql
// Allows flexible querying for complex scenarios
// Particularly useful for the web UI to reduce round trips
```

**Action Items**:
- [ ] Evaluate HotChocolate GraphQL library
- [ ] Create GraphQL schema for core entities
- [ ] Implement resolvers for name generation
- [ ] Add GraphQL playground
- [ ] Document GraphQL API separately
- [ ] Keep REST API as primary, GraphQL as alternative

#### 3.2 Minimal APIs (Alternative Endpoints)
```csharp
// Create simple, performant endpoints for high-traffic operations
app.MapPost("/api/fast/generate-name", async (
    [FromBody] SimpleNameRequest request,
    IResourceNamingRequestService service) =>
{
    var result = await service.GenerateNameFastAsync(request);
    return Results.Ok(result);
})
.RequireRateLimiting("api")
.WithOpenApi();
```

**Action Items**:
- [ ] Identify high-traffic endpoints
- [ ] Create minimal API endpoints for performance
- [ ] Benchmark performance improvements
- [ ] Document minimal API endpoints

#### 3.3 Webhook Support
```csharp
// Allow users to register webhooks for async operations
// POST /api/v2/webhooks/register
// POST /api/v2/names/generate-async (returns operation ID, calls webhook when complete)
```

**Action Items**:
- [ ] Design webhook registration system
- [ ] Implement webhook delivery with retries
- [ ] Add webhook signature verification
- [ ] Create webhook testing tools
- [ ] Document webhook format and events

#### 3.4 Batch Operations
```csharp
// POST /api/v2/names/generate-batch
{
    "requests": [
        { "resourceType": "vm", "location": "eastus", ... },
        { "resourceType": "sa", "location": "westus", ... }
    ],
    "continueOnError": true
}

// Response with partial success handling
{
    "results": [...],
    "successCount": 95,
    "failureCount": 5,
    "errors": [...]
}
```

**Action Items**:
- [ ] Design batch operation API
- [ ] Implement batch processing with error handling
- [ ] Add batch operation limits
- [ ] Document batch endpoints

### Phase 4: Enhanced Security & Performance

**Goal**: Harden API security and optimize performance

#### 4.1 OAuth 2.0 / Azure AD Integration (Optional)
```csharp
// Add support for OAuth 2.0 alongside API keys
// Allows enterprise SSO integration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["AzureAd:Authority"];
        options.Audience = configuration["AzureAd:ClientId"];
    });
```

**Action Items**:
- [ ] Design authentication strategy (support both API keys and OAuth)
- [ ] Implement JWT bearer token support
- [ ] Add Azure AD integration option
- [ ] Update API key attribute to support multiple auth schemes
- [ ] Document OAuth configuration

#### 4.2 Advanced Caching
```csharp
// Add output caching for expensive operations
app.MapGet("/api/v2/ResourceTypes", async (IResourceTypeService service) =>
{
    var types = await service.GetItemsAsync();
    return Results.Ok(types);
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(10)));
```

**Action Items**:
- [ ] Implement output caching for read endpoints
- [ ] Add ETag support for conditional requests
- [ ] Implement cache invalidation strategy
- [ ] Add cache-control headers

#### 4.3 Request Validation & Sanitization
**Action Items**:
- [ ] Add request size limits middleware
- [ ] Implement input validation with FluentValidation
- [ ] Add request sanitization for XSS prevention
- [ ] Create validation error responses with field-level details

### Phase 5: Developer Tooling

**Goal**: Improve developer experience with better tooling

#### 5.1 Client SDK Generation
**Action Items**:
- [ ] Configure NSwag or Kiota for client generation
- [ ] Generate C# SDK
- [ ] Generate TypeScript SDK
- [ ] Generate PowerShell module
- [ ] Publish SDKs to package managers (NuGet, npm)
- [ ] Create SDK documentation and samples

#### 5.2 Testing Tools
**Action Items**:
- [ ] Create comprehensive Postman collection
- [ ] Add API integration tests
- [ ] Create load testing scenarios
- [ ] Document testing approach

#### 5.3 API Portal
**Action Items**:
- [ ] Create dedicated API documentation website
- [ ] Add interactive API explorer
- [ ] Include code samples and tutorials
- [ ] Add API key management UI

## Migration Timeline

### Immediate (1-2 weeks)
- ✅ Create this migration plan
- Phase 1.2: Add response type attributes
- Phase 1.3: Implement rate limiting
- Phase 1.5: Document current API

### Short Term (1-2 months)
- Phase 1.1: Standardize responses
- Phase 1.4: Enhanced logging
- Phase 2.1: Implement versioning
- Phase 4.2: Add caching

### Medium Term (3-6 months)
- Phase 2.2: Build v2 API with improvements
- Phase 3.4: Batch operations
- Phase 5.1: Generate client SDKs

### Long Term (6-12 months)
- Phase 3.1: GraphQL API (if demand exists)
- Phase 3.3: Webhook support
- Phase 4.1: OAuth integration (enterprise feature)
- Phase 5.3: API portal

## Backward Compatibility Guarantee

### What We WILL NOT Break
1. **Existing endpoint URLs** (`/api/ResourceTypes`, `/api/ResourceNamingRequests`, etc.)
2. **Current request/response formats** (maintain existing JSON structure)
3. **API key authentication** (current three-tier system continues to work)
4. **HTTP methods** (GET, POST, DELETE remain the same)

### What We WILL Add (Non-Breaking)
1. **Optional features** (versioned endpoints, new response format opt-in)
2. **New endpoints** (v2 APIs, GraphQL, webhooks)
3. **Additional response headers** (rate limiting, correlation IDs)
4. **Enhanced error details** (more descriptive, but existing format still works)

### Deprecation Policy
When we need to deprecate an API feature:
1. **Announce** deprecation with 6-month notice minimum
2. **Document** in API changelog and Swagger
3. **Add headers** to deprecated endpoints (`Sunset`, `Deprecation`)
4. **Provide migration path** in documentation
5. **Maintain** deprecated endpoints for 12 months minimum after announcement

## Technical Recommendations

### Recommended Packages
```xml
<!-- API Versioning -->
<PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />

<!-- Rate Limiting (built into .NET 8) -->
<!-- No additional package needed -->

<!-- Enhanced Swagger -->
<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.8.1" />

<!-- GraphQL (if implemented) -->
<PackageReference Include="HotChocolate.AspNetCore" Version="13.9.12" />

<!-- Client SDK Generation (optional) -->
<PackageReference Include="NSwag.AspNetCore" Version="14.1.0" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- Application Insights (optional) -->
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
```

### Architecture Patterns

#### Middleware Stack
```
Request
  ↓
CorrelationId Middleware (add X-Correlation-ID)
  ↓
Request Logging Middleware (log incoming)
  ↓
Rate Limiting Middleware (check limits)
  ↓
Authentication Middleware (API key / JWT)
  ↓
Authorization Middleware (check permissions)
  ↓
Response Caching Middleware (check cache)
  ↓
Controller / Endpoint
  ↓
Result Filter (wrap in ApiResponse)
  ↓
Exception Filter (catch and format errors)
  ↓
Response Logging Middleware (log outgoing)
  ↓
Response
```

#### Controller Pattern (V2)
```csharp
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
[ApiKey]
public class ResourceTypesV2Controller : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ResourceType>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status429TooManyRequests)]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<ApiResponse<List<ResourceType>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? filter = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPagedAsync(page, pageSize, filter, sort, cancellationToken);
        
        return Ok(new ApiResponse<List<ResourceType>>
        {
            Success = true,
            Data = result.Items,
            Metadata = new ApiMetadata
            {
                RequestId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Version = "2.0",
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = result.TotalCount,
                    TotalPages = result.TotalPages
                }
            }
        });
    }
}
```

## Success Metrics

### Phase 1 Success Criteria
- [ ] 100% of endpoints have response type attributes
- [ ] All API calls include correlation ID
- [ ] Rate limiting prevents abuse (zero service degradation incidents)
- [ ] API documentation covers 100% of endpoints
- [ ] Postman collection available for all operations

### Phase 2 Success Criteria
- [ ] v1 and v2 APIs coexist without breaking changes
- [ ] Swagger UI shows both versions clearly
- [ ] Zero complaints from existing API users about breaking changes
- [ ] v2 adoption > 10% within 3 months of release

### Phase 3 Success Criteria
- [ ] GraphQL API handles complex queries efficiently
- [ ] Batch operations reduce API call volume by 30%+
- [ ] Webhook delivery success rate > 99%

### Overall Success Metrics
- **Performance**: API response time P95 < 200ms for name generation
- **Reliability**: API availability > 99.9%
- **Security**: Zero unauthorized access incidents
- **Developer Experience**: Positive feedback from API users (survey)
- **Adoption**: 50%+ of users on v2 API within 6 months

## Risk Mitigation

### Risk: Breaking Existing Integrations
**Mitigation**:
- Maintain v1 API indefinitely
- Comprehensive testing before releases
- Staged rollout with canary deployments
- Clear communication of changes

### Risk: Performance Degradation
**Mitigation**:
- Load testing before production deployment
- Gradual rollout of new features
- Performance monitoring and alerts
- Ability to disable new features if needed

### Risk: Complexity Increase
**Mitigation**:
- Keep v1 simple, add complexity only in v2
- Clear code organization and documentation
- Team training on new patterns
- Automated testing for all scenarios

## Communication Plan

### For Existing Users
1. **Announcement**: Blog post and GitHub release notes
2. **Documentation**: Update wiki with migration guide
3. **Support**: Dedicated support channel for migration questions
4. **Samples**: Before/after code samples for common scenarios

### For New Users
1. **Getting Started**: Updated quickstart guide
2. **Examples**: Comprehensive code samples
3. **SDKs**: Pre-built client libraries
4. **Videos**: Video tutorials for common scenarios

## Conclusion

This migration plan provides a roadmap for modernizing the Azure Naming Tool API while respecting existing integrations. The phased approach allows us to:

1. **Improve incrementally** without disruption
2. **Add value** through new features and better developer experience
3. **Maintain compatibility** with existing users
4. **Stay current** with modern API best practices
5. **Scale confidently** for future growth

The key principle is **additive enhancement**: we add modern features alongside the existing API, giving users time to migrate at their own pace while continuing to support legacy integrations.

### Next Steps
1. ✅ Review and approve this plan
2. Create GitHub issues for Phase 1 tasks
3. Prioritize tasks based on user feedback and impact
4. Begin implementation with Phase 1.2 (low-risk, high-value improvements)
5. Schedule regular reviews to assess progress and adjust priorities

---

**Document Version**: 1.0  
**Created**: October 21, 2025  
**Author**: Azure Naming Tool Development Team  
**Status**: Proposed - Awaiting Approval

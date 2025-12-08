# Azure Tenant Name Validation Implementation Plan

## Overview

This document outlines the implementation plan for integrating Azure tenant name validation into the Azure Naming Tool. This feature will allow users to optionally check if a generated resource name already exists in their Azure tenant before using it.

**Status**: Planning Phase  
**Target Version**: v5.1.0  
**Complexity**: High  
**Priority**: Medium

---

## Business Requirements

### Core Functionality
- **Optional Feature**: Users can choose whether to enable Azure tenant validation
- **Name Existence Check**: Query Azure Resource Graph to check if a generated name already exists
- **Conflict Resolution**: Configurable behavior when a name conflict is detected
- **Multi-Tenant Support**: Support for users with multiple Azure tenants
- **API Integration**: Available for both web UI and API endpoints

### User Stories
1. As an administrator, I want to optionally connect the naming tool to my Azure tenant so I can validate names against existing resources
2. As a user, I want to be notified when a generated name already exists so I can avoid deployment conflicts
3. As an administrator, I want to configure what happens when a name conflict occurs (auto-increment, notify, or ignore)
4. As a user generating names via API, I want the same validation capabilities available in the web interface
5. As an administrator, I don't want to be forced to connect to Azure if my organization doesn't need this feature

---

## Technical Architecture

### 1. Authentication & Authorization Strategy

#### Option A: Azure Managed Identity (Recommended for Azure-hosted deployments)
**Pros:**
- No credential storage required
- Automatic credential rotation
- Best security practice for Azure-hosted apps
- Works seamlessly in App Service, Container Apps, AKS

**Cons:**
- Only works when app is hosted in Azure
- Requires proper RBAC assignments
- Not available for on-premises/local deployments

**RBAC Requirements:**
- Minimum: `Reader` role at subscription or management group level
- Recommended: Custom role with only `Microsoft.ResourceGraph/resources/read` permission

#### Option B: Service Principal with Client Secret
**Pros:**
- Works anywhere (Azure, on-premises, local)
- Can be scoped to specific subscriptions
- Explicit control over permissions

**Cons:**
- Requires secure secret storage
- Secrets expire and need rotation
- More complex setup for users

**Implementation:**
- Store Client ID in configuration (not sensitive)
- Store Client Secret in Azure Key Vault or encrypted configuration
- Provide clear documentation on creating service principals

#### Option C: User-Delegated Authentication (Interactive)
**Pros:**
- Uses user's own permissions
- No service principal needed
- Clear audit trail (user identity)

**Cons:**
- Not suitable for API scenarios
- Requires interactive login flow
- Token refresh complexity
- Not ideal for unattended operations

#### **Recommended Approach: Hybrid**
- **Default**: Managed Identity (when available)
- **Fallback**: Service Principal (when configured)
- **Config-driven**: Let administrators choose authentication method

### 2. Credential Storage Options

#### Option 1: Azure Key Vault Integration (Recommended)
```json
{
  "AzureValidation": {
    "Enabled": false,
    "AuthenticationMode": "ManagedIdentity|ServicePrincipal",
    "KeyVaultUri": "https://mykeyvault.vault.azure.net/",
    "ServicePrincipal": {
      "TenantId": "guid",
      "ClientId": "guid",
      "ClientSecretName": "naming-tool-sp-secret"
    }
  }
}
```

**Pros:**
- Industry standard for secret management
- Automatic encryption at rest
- Access logging and auditing
- Secret versioning and rotation

**Cons:**
- Requires Azure Key Vault resource
- Additional cost (~$0.03/10k operations)
- More complex setup

#### Option 2: Encrypted Configuration File
```json
{
  "AzureValidation": {
    "Enabled": false,
    "AuthenticationMode": "ServicePrincipal",
    "ServicePrincipal": {
      "TenantId": "guid",
      "ClientId": "guid",
      "ClientSecret": "encrypted:AQAAAAEAACcQAAAA..."
    }
  }
}
```

**Pros:**
- No additional Azure resources required
- Works in any environment
- Simpler setup for small deployments

**Cons:**
- Need to implement encryption/decryption
- Key management for encryption key
- Less secure than Key Vault

#### Option 3: Environment Variables
```bash
AZURE_VALIDATION_ENABLED=true
AZURE_TENANT_ID=guid
AZURE_CLIENT_ID=guid
AZURE_CLIENT_SECRET=secret
```

**Pros:**
- Standard practice for containerized apps
- No file-based storage
- Easy to configure in App Service, Kubernetes

**Cons:**
- Secrets visible in process environment
- No built-in rotation
- Requires restart to change

#### **Recommended Approach: Tiered**
1. **Production**: Azure Key Vault
2. **Development**: Environment Variables
3. **On-Premises**: Encrypted Configuration File

### 3. Name Validation Logic

#### Understanding Resource Name Scopes

Azure resources have different scopes of uniqueness based on the Microsoft documentation:

| Scope | Description | Example Resources |
|-------|-------------|-------------------|
| **Global** | Unique across ALL of Azure (all customers worldwide) | Storage Accounts, App Services, Key Vault, Container Registry, Cosmos DB, Redis Cache, Service Bus, Event Hub |
| **Resource Group** | Unique within a resource group only | Virtual Networks, VMs, NSGs, Public IPs |
| **Subscription** | Unique within a subscription | Resource Groups |
| **Region** | Unique within an Azure region | Batch Accounts |
| **Parent Resource** | Unique within a parent resource | Subnets (within VNet), Disks |

**CRITICAL LIMITATION**: Azure Resource Graph can only query resources **you have access to** (your subscriptions). It **CANNOT** check if a globally unique name is already taken by another Azure customer.

#### Validation Approaches by Scope

**For GLOBALLY UNIQUE resources** (Storage, App Service, Key Vault, etc.):
- **Option 1 - Check Name Availability API** (Recommended): Use Azure's `CheckNameAvailability` API which checks true global uniqueness
  ```csharp
  // Example for Storage Account
  POST https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Storage/checkNameAvailability?api-version=2023-01-01
  {
    "name": "mystorageaccount",
    "type": "Microsoft.Storage/storageAccounts"
  }
  // Returns: { "nameAvailable": false, "reason": "AlreadyExists", "message": "..." }
  ```
- **Option 2 - Resource Graph** (Limited): Only checks within your tenant - won't catch conflicts from other customers
- **Recommended**: Use CheckNameAvailability API for global resources, Resource Graph for scoped resources

**For RESOURCE GROUP/SUBSCRIPTION scoped resources** (VNets, VMs, etc.):
- Resource Graph queries work perfectly
- These resources don't need global uniqueness checks

#### Implementation Strategy

The existing `resourcetypes.json` contains a `scope` field with values:
- `"global"` - Requires CheckNameAvailability API
- `"resource group"` - Use Resource Graph
- `"subscription"` - Use Resource Graph
- `"region"` - Use Resource Graph (with region filter)
- `"parent resource"` - Not applicable for name generation

**Validation Flow:**
```
1. Generate resource name using existing logic
2. IF Azure validation enabled:
   a. Load ResourceType from resourcetypes.json
   b. Check ResourceType.Scope property
   c. IF Scope == "global":
      - Call CheckNameAvailability API (true global check)
   d. ELSE:
      - Query Resource Graph (tenant-scoped check)
   e. IF name exists:
      - Apply conflict resolution strategy
   f. Return final name with validation metadata
```

#### Azure Resource Graph Query
```kusto
Resources
| where name =~ '{generatedName}'
| where type =~ '{resourceType}'
| project id, name, type, resourceGroup, subscriptionId, location
```

**Query Scopes:**
- Single Subscription
- Multiple Subscriptions (comma-separated list)
- Management Group (all subscriptions under MG)

#### Validation Flow
```
1. Generate resource name using existing logic
2. IF Azure validation enabled:
   a. Authenticate to Azure (Managed Identity or Service Principal)
   b. Query Resource Graph for name existence
   c. IF name exists:
      - Check conflict resolution setting
      - Apply resolution (auto-increment, notify, fail)
   d. IF name not found:
      - Return generated name
3. Return final name with validation metadata
```

#### Performance Considerations
- **Caching**: Cache validation results for 5 minutes to reduce API calls
- **Batch Queries**: For bulk operations, query multiple names in single request
- **Timeout**: Set 5-second timeout for Azure queries to avoid blocking
- **Retry Logic**: Implement exponential backoff for transient failures

### 4. Conflict Resolution Strategies

#### Strategy 1: Auto-Increment (Default)
```
Original: vnet-prod-001
If exists: vnet-prod-002
If exists: vnet-prod-003
...up to max attempts (default: 100)
```

**Configuration:**
```json
{
  "ConflictResolution": {
    "Strategy": "AutoIncrement",
    "MaxAttempts": 100,
    "IncrementPadding": 3
  }
}
```

#### Strategy 2: Notify Only
```
Return: 
{
  "name": "vnet-prod-001",
  "existsInAzure": true,
  "warning": "This name already exists in your Azure tenant"
}
```

**Use Case:** User wants to know but will handle manually

#### Strategy 3: Fail
```
Return: 400 Bad Request
{
  "error": "Name conflict: vnet-prod-001 already exists in Azure"
}
```

**Use Case:** Strict enforcement, no duplicates allowed

#### Strategy 4: Suffix Uniqueness
```
Original: vnet-prod-001
If exists: vnet-prod-001-a1b2c3 (add random suffix)
```

**Use Case:** When incrementing doesn't fit naming convention

#### **Recommended Default**: Auto-Increment with configurable max attempts

---

## Data Models

### Configuration Model
```csharp
public class AzureValidationSettings
{
    public bool Enabled { get; set; } = false;
    public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.ManagedIdentity;
    public string TenantId { get; set; } = string.Empty;
    public List<string> SubscriptionIds { get; set; } = new();
    public string? ManagementGroupId { get; set; }
    public ServicePrincipalSettings? ServicePrincipal { get; set; }
    public KeyVaultSettings? KeyVault { get; set; }
    public ConflictResolutionSettings ConflictResolution { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
}

public class ServicePrincipalSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; } // Direct or encrypted
    public string? ClientSecretKeyVaultName { get; set; } // Key Vault reference
}

public class KeyVaultSettings
{
    public string KeyVaultUri { get; set; } = string.Empty;
    public string ClientSecretName { get; set; } = "naming-tool-client-secret";
}

public class ConflictResolutionSettings
{
    public ConflictStrategy Strategy { get; set; } = ConflictStrategy.AutoIncrement;
    public int MaxAttempts { get; set; } = 100;
    public int IncrementPadding { get; set; } = 3;
    public bool IncludeWarnings { get; set; } = true;
}

public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    public int DurationMinutes { get; set; } = 5;
}

public enum AuthenticationMode
{
    ManagedIdentity,
    ServicePrincipal
}

public enum ConflictStrategy
{
    AutoIncrement,
    NotifyOnly,
    Fail,
    SuffixRandom
}
```

### Response Model Enhancement
```csharp
public class ResourceNameResponse
{
    // Existing properties...
    
    // New validation metadata
    public AzureValidationMetadata? ValidationMetadata { get; set; }
}

public class AzureValidationMetadata
{
    public bool ValidationPerformed { get; set; } = false;
    public bool ExistsInAzure { get; set; } = false;
    public string? OriginalName { get; set; } // If auto-incremented
    public int? IncrementAttempts { get; set; }
    public List<string>? ConflictingResources { get; set; } // Resource IDs that matched
    public string? ValidationWarning { get; set; }
    public DateTime? ValidationTimestamp { get; set; }
}
```

---

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
**Goal**: Core infrastructure without Azure integration

- [ ] Create `AzureValidationSettings` configuration model
- [ ] Add configuration UI in Admin section
  - Enable/Disable toggle
  - Authentication mode selection
  - Subscription ID input
  - Conflict resolution strategy selection
- [ ] Create `IAzureValidationService` interface
- [ ] Implement mock validation service for testing
- [ ] Add validation metadata to response models
- [ ] Unit tests for configuration and models

**Deliverables:**
- Configuration models and UI
- Service interface defined
- Mock implementation for testing
- Updated response models with validation metadata

### Phase 2: Azure Integration (Week 3-4)
**Goal**: Implement actual Azure Resource Graph queries

- [ ] Install NuGet packages:
  - `Azure.Identity` (for authentication)
  - `Azure.ResourceManager` (for resource queries)
  - `Azure.ResourceManager.ResourceGraph` (for graph queries)
- [ ] Implement `AzureValidationService`:
  - Managed Identity authentication
  - Service Principal authentication
  - Resource Graph query execution
  - Result parsing and mapping
- [ ] Implement credential providers:
  - Environment variable provider
  - Configuration file provider (with encryption)
  - Azure Key Vault provider (optional)
- [ ] Add caching layer using existing `CacheHelper`
- [ ] Error handling and logging
- [ ] Integration tests with test Azure subscription

**Deliverables:**
- Working Azure authentication
- Resource Graph query implementation
- Credential management
- Caching layer

### Phase 3: Conflict Resolution (Week 5)
**Goal**: Implement all conflict resolution strategies

- [ ] Auto-increment logic:
  - Parse existing instance number
  - Increment with proper padding
  - Validate incremented name doesn't exceed length limits
  - Recursive validation up to max attempts
- [ ] Notify-only strategy
- [ ] Fail strategy
- [ ] Random suffix strategy
- [ ] Strategy factory pattern
- [ ] Unit tests for each strategy
- [ ] Performance testing (batch validation)

**Deliverables:**
- All 4 conflict resolution strategies working
- Strategy selection based on configuration
- Performance optimization

### Phase 4: API Integration (Week 6)
**Goal**: Integrate validation into existing endpoints

- [ ] Update V2 `ResourceNamingRequestsController.Generate`:
  - Call validation service after name generation
  - Apply conflict resolution
  - Include validation metadata in response
- [ ] Update V2 `GenerateBulk` endpoint:
  - Batch validation queries
  - Parallel validation for performance
  - Aggregate validation results
- [ ] Add query parameter `skipAzureValidation=true` for opt-out
- [ ] Update API documentation (Swagger)
- [ ] API integration tests

**Deliverables:**
- V2 API endpoints with validation
- Opt-out capability
- Updated Swagger documentation
- Integration tests

### Phase 5: UI Integration (Week 7)
**Goal**: Integrate validation into web interface

- [ ] Update Generate Name page:
  - Show validation status (checking, exists, available)
  - Display validation warnings
  - Show original vs. incremented name
  - Visual indicators (icons, colors)
- [ ] Add admin configuration page:
  - Azure connection settings
  - Test connection button
  - Conflict resolution settings
  - Subscription management
- [ ] Add validation status to history/log
- [ ] Loading states and error messages
- [ ] UI/UX testing

**Deliverables:**
- Updated Generate Name UI with validation
- Admin configuration UI
- Test connection functionality
- Visual feedback for users

### Phase 6: Security & Documentation (Week 8)
**Goal**: Security hardening and comprehensive documentation

- [ ] Security review:
  - Credential encryption implementation
  - RBAC permission documentation
  - Secure Key Vault integration
  - Audit logging for validation queries
- [ ] Documentation:
  - Administrator setup guide
  - Service Principal creation steps
  - Managed Identity configuration
  - RBAC permission requirements
  - Troubleshooting guide
  - API documentation updates
- [ ] Performance optimization:
  - Query result caching
  - Batch query optimization
  - Timeout handling
- [ ] Final testing:
  - End-to-end scenarios
  - Security testing
  - Performance testing

**Deliverables:**
- Security hardening complete
- Comprehensive documentation
- Performance optimizations
- Production-ready feature

---

## Configuration Examples

### Example 1: Managed Identity (Azure-hosted)
```json
{
  "AzureValidation": {
    "Enabled": true,
    "AuthMode": "ManagedIdentity",
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "SubscriptionIds": [
      "87654321-4321-4321-4321-210987654321"
    ],
    "ConflictResolution": {
      "Strategy": "AutoIncrement",
      "MaxAttempts": 100,
      "IncludeWarnings": true
    },
    "Cache": {
      "Enabled": true,
      "DurationMinutes": 5
    }
  }
}
```

**Setup Steps:**
1. Enable Managed Identity on App Service/Container App
2. Assign `Reader` role to Managed Identity at subscription level
3. Configure subscription IDs in settings
4. Enable validation in admin UI

### Example 2: Service Principal with Key Vault
```json
{
  "AzureValidation": {
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "SubscriptionIds": [
      "87654321-4321-4321-4321-210987654321",
      "11111111-2222-3333-4444-555555555555"
    ],
    "ServicePrincipal": {
      "ClientId": "abcdef12-3456-7890-abcd-ef1234567890"
    },
    "KeyVault": {
      "KeyVaultUri": "https://naming-tool-kv.vault.azure.net/",
      "ClientSecretName": "sp-naming-tool-secret"
    },
    "ConflictResolution": {
      "Strategy": "AutoIncrement",
      "MaxAttempts": 50
    }
  }
}
```

**Setup Steps:**
1. Create Service Principal: `az ad sp create-for-rbac --name "naming-tool-validator"`
2. Assign `Reader` role: `az role assignment create --assignee <sp-id> --role Reader --subscription <sub-id>`
3. Store client secret in Key Vault
4. Grant app access to Key Vault (Managed Identity or separate SP)
5. Configure settings

### Example 3: Disabled (Default)
```json
{
  "AzureValidation": {
    "Enabled": false
  }
}
```

No Azure connection required. App functions as it does today.

---

## Security Considerations

### 1. Principle of Least Privilege
- **Minimum Permission**: `Microsoft.ResourceGraph/resources/read`
- **Scope**: Limit to specific subscriptions, not entire tenant
- **Custom Role Definition**:
```json
{
  "Name": "Naming Tool Validator",
  "Description": "Read-only access for name validation queries",
  "Actions": [
    "Microsoft.ResourceGraph/resources/read"
  ],
  "AssignableScopes": [
    "/subscriptions/{subscription-id}"
  ]
}
```

### 2. Credential Protection
- **Never log secrets**: Ensure no credentials in logs
- **Encryption at rest**: All secrets encrypted in storage
- **Encryption in transit**: HTTPS for all Azure API calls
- **Rotation**: Document secret rotation procedures
- **Key Vault**: Use Azure Key Vault for production

### 3. Audit Trail
- Log all validation queries (name, result, timestamp)
- Log authentication attempts
- Track which user/API key triggered validation
- Store validation results in history

### 4. Rate Limiting
- Azure Resource Graph limits: 15 requests per 5 seconds
- Implement request throttling
- Queue requests during high load
- Provide feedback when throttled

### 5. Data Privacy
- Don't expose subscription IDs to non-admin users
- Don't return full resource IDs in API responses (only name existence)
- Admin-only access to Azure connection settings

---

## API Changes

### V2 Generate Endpoint Enhancement
```http
POST /api/v2/ResourceNamingRequests/Generate
```

**Request** (unchanged):
```json
{
  "resourceType": "vnet",
  "resourceLocation": "use",
  "resourceInstance": "001",
  "createdBy": "user@example.com"
}
```

**Response** (enhanced):
```json
{
  "success": true,
  "data": {
    "resourceName": "vnet-use-002",
    "message": "Name generated successfully",
    "resourceNameDetails": {
      // existing fields...
    },
    "validationMetadata": {
      "validationPerformed": true,
      "existsInAzure": true,
      "originalName": "vnet-use-001",
      "incrementAttempts": 2,
      "conflictingResources": [
        "/subscriptions/.../resourceGroups/rg-prod/providers/Microsoft.Network/virtualNetworks/vnet-use-001"
      ],
      "validationWarning": "Original name existed, auto-incremented to vnet-use-002",
      "validationTimestamp": "2025-10-27T10:30:00Z"
    }
  }
}
```

**New Query Parameters:**
- `skipAzureValidation=true`: Bypass validation for this request
- `conflictStrategy=AutoIncrement|NotifyOnly|Fail`: Override global strategy

### V2 GenerateBulk Endpoint Enhancement
```http
POST /api/v2/ResourceNamingRequests/GenerateBulk
```

**Response** (enhanced):
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "resourceType": "vnet",
        "success": true,
        "resourceName": "vnet-use-002",
        "validationMetadata": {
          "validationPerformed": true,
          "existsInAzure": true,
          "originalName": "vnet-use-001",
          "incrementAttempts": 2
        }
      }
    ],
    "totalRequested": 3,
    "successCount": 3,
    "failureCount": 0,
    "validationSummary": {
      "totalValidated": 3,
      "conflictsFound": 1,
      "autoIncremented": 1,
      "avgValidationTimeMs": 245
    }
  }
}
```

### New Admin Endpoint: Test Connection
```http
POST /api/v2/Admin/TestAzureConnection
```

**Response:**
```json
{
  "success": true,
  "data": {
    "authenticated": true,
    "authenticationMode": "ManagedIdentity",
    "tenantId": "12345678-1234-1234-1234-123456789012",
    "accessibleSubscriptions": [
      {
        "subscriptionId": "87654321-4321-4321-4321-210987654321",
        "displayName": "Production",
        "hasReadAccess": true
      }
    ],
    "resourceGraphAccess": true,
    "testQuerySucceeded": true,
    "message": "Successfully connected to Azure"
  }
}
```

---

## UI Changes

### 1. Admin Configuration Page
**Location**: `/configuration/azurevalidation`

**Sections:**
- **Enable/Disable Toggle**
  - Clear warning: "This feature requires Azure credentials"
  
- **Authentication Settings**
  - Radio buttons: Managed Identity | Service Principal
  - If Service Principal:
    - Tenant ID input
    - Client ID input
    - Client Secret input (password field)
    - Key Vault URI (optional)
  
- **Scope Settings**
  - Subscription IDs (multi-line text or tag input)
  - Management Group ID (optional, advanced)
  
- **Conflict Resolution**
  - Strategy dropdown: Auto-Increment | Notify Only | Fail | Random Suffix
  - Max Attempts slider (1-1000) - only for Auto-Increment
  - Increment Padding (1-5 digits)
  
- **Cache Settings**
  - Enable cache toggle
  - Duration slider (1-60 minutes)
  
- **Test Connection Button**
  - Validates credentials
  - Checks subscription access
  - Tests Resource Graph query
  - Shows success/error message

### 2. Generate Name Page Enhancement
**Location**: `/generate`

**Changes:**
- Add validation status indicator:
  - 🔍 Checking Azure...
  - ✅ Available in Azure
  - ⚠️ Exists (auto-incremented)
  - ❌ Conflict (manual resolution needed)
  
- Show validation metadata:
  - "Original name: vnet-use-001 (exists in Azure)"
  - "Auto-incremented to: vnet-use-002"
  - "Validated at: 10:30 AM"
  
- Add "Skip Azure Check" checkbox for quick generation

### 3. History/Log Enhancement
**Location**: `/history` or `/log`

**Changes:**
- Add "Azure Status" column:
  - Validated ✓
  - Existed (incremented) ⚠️
  - Not validated -
  
- Filter by validation status
- Show validation details in expanded view

---

## Testing Strategy

### 1. Unit Tests
- Configuration model validation
- Authentication provider selection logic
- Conflict resolution strategies
- Name increment logic
- Cache behavior

### 2. Integration Tests
- Azure Resource Graph queries (requires test subscription)
- Managed Identity authentication (in Azure environment)
- Service Principal authentication
- Key Vault secret retrieval
- End-to-end name validation flow

### 3. Performance Tests
- Single name validation latency (target: <500ms)
- Bulk validation (100 names) throughput (target: <5s)
- Cache hit rate (target: >80% for repeated queries)
- Concurrent request handling
- Azure Resource Graph rate limit handling

### 4. Security Tests
- Credential encryption/decryption
- Secret not exposed in logs
- Unauthorized access attempts
- Invalid credentials handling
- RBAC permission validation

### 5. User Acceptance Tests
- Admin configuration workflow
- Test connection functionality
- Generate name with validation
- Bulk generation with validation
- Conflict resolution scenarios
- Error message clarity

---

## Migration & Rollout

### Phase 1: Opt-In Beta (v5.1.0-beta)
- Feature disabled by default
- Documentation for early adopters
- Feedback collection
- Performance monitoring

### Phase 2: General Availability (v5.1.0)
- Feature available but still disabled by default
- Comprehensive documentation
- Admin UI for easy setup
- Support for common scenarios

### Phase 3: Encouraged Adoption (v5.2.0)
- Prompt users to enable if running in Azure
- Success stories and case studies
- Performance improvements based on usage data

### Backward Compatibility
- Feature is completely optional
- No breaking changes to existing APIs
- Response models extended, not replaced
- Existing behavior unchanged when disabled

---

## Risk Assessment & Mitigation

### Risk 1: Azure API Latency
**Impact**: Slow name generation  
**Probability**: Medium  
**Mitigation:**
- Implement aggressive caching (5-minute default)
- Add timeout (5 seconds)
- Provide "Skip Validation" option
- Async validation for UI (don't block)

### Risk 2: Authentication Complexity
**Impact**: Users struggle to set up  
**Probability**: High  
**Mitigation:**
- Comprehensive documentation with screenshots
- "Test Connection" button with clear error messages
- Support for multiple auth methods
- Default to simpler options (environment variables)

### Risk 3: Azure Resource Graph Limits
**Impact**: Rate limiting during bulk operations  
**Probability**: Low  
**Mitigation:**
- Batch queries (up to 1000 names per query)
- Implement queue system
- Exponential backoff retry
- Clear error messages to users

### Risk 4: Security Vulnerabilities
**Impact**: Credential exposure  
**Probability**: Low  
**Mitigation:**
- Mandatory encryption for stored secrets
- Key Vault recommendation for production
- Security audit before GA release
- Clear security documentation

### Risk 5: Cost Concerns
**Impact**: Azure API costs accumulate  
**Probability**: Low  
**Mitigation:**
- Resource Graph queries are very cheap (~$0.0005 per query)
- Caching reduces query count by 80%+
- Document expected costs in setup guide
- Provide cost calculator

### Risk 6: False Positives/Negatives
**Impact**: Wrong validation results  
**Probability**: Low  
**Mitigation:**
- Resource Graph is authoritative source
- Include timestamp in results
- Cache expiration (5 minutes)
- Allow manual override

---

## Success Metrics

### Adoption Metrics
- % of installations with validation enabled
- % of name generation requests that use validation
- Number of admin configurations completed

### Performance Metrics
- Average validation latency (target: <500ms)
- Cache hit rate (target: >80%)
- Azure API error rate (target: <1%)
- Bulk validation throughput (target: >20 names/second)

### Business Metrics
- User satisfaction score for validation feature
- Reduction in Azure deployment name conflicts
- Support ticket reduction related to name conflicts
- Documentation page views

### Technical Metrics
- Authentication failure rate (target: <5%)
- Validation timeout rate (target: <2%)
- Auto-increment success rate (target: >95%)
- API response time impact (target: <10% increase)

---

## Documentation Requirements

### 1. Administrator Setup Guide
- Prerequisites (Azure subscription, permissions)
- Creating service principal step-by-step
- Configuring Managed Identity
- Setting up Key Vault (optional)
- Assigning RBAC permissions
- Configuration file examples
- Troubleshooting common setup issues

### 2. User Guide
- How to use validation in web UI
- Understanding validation results
- When to skip validation
- API examples with validation enabled
- Conflict resolution behavior

### 3. API Documentation
- Updated Swagger documentation
- Request/response examples
- Query parameters
- Validation metadata structure
- Error codes and messages

### 4. Security Guide
- Authentication methods comparison
- Credential storage best practices
- Key Vault integration
- RBAC permissions
- Audit logging

### 5. Troubleshooting Guide
- Common error messages and solutions
- Authentication issues
- Permission problems
- Performance issues
- Azure Resource Graph limitations

---

## Open Questions & Decisions Needed

### Decision 1: Default Conflict Strategy
**Options:**
- A) Auto-Increment (user-friendly)
- B) Notify Only (gives control)
- C) Fail (strict enforcement)

**Recommendation**: Auto-Increment - most user-friendly, aligns with tool's purpose

### Decision 2: Authentication Priority
**Options:**
- A) Managed Identity first (best for Azure)
- B) Service Principal first (works everywhere)

**Recommendation**: Managed Identity first with Service Principal fallback

### Decision 3: Scope of Validation
**Options:**
- A) Single subscription only (simple)
- B) Multiple subscriptions (flexible)
- C) Management Group support (enterprise)

**Recommendation**: Start with B (multiple subscriptions), add C later if needed

### Decision 4: UI Placement
**Options:**
- A) Always show validation status (prominent)
- B) Collapsible section (less clutter)
- C) Tooltip/icon only (minimal)

**Recommendation**: A for Generate page, C for bulk operations

### Decision 5: Cache Scope
**Options:**
- A) Global cache (all users share)
- B) Per-user cache (isolated)
- C) Per-session cache (temporary)

**Recommendation**: A (global) - Resource existence is same for all users

### Decision 6: Validation in Bulk Operations
**Options:**
- A) Validate all names sequentially
- B) Batch validate (single Azure query)
- C) Make validation optional for bulk

**Recommendation**: B (batch validate) for performance

---

## Cost Analysis

### Azure Resource Graph Pricing
- **Per Query**: ~$0.0005 (half a cent per 1000 queries)
- **Monthly Estimate** (1000 validations/day):
  - Without cache: $15/month (30,000 queries)
  - With 80% cache hit rate: $3/month (6,000 queries)
  
### Azure Key Vault Pricing (Optional)
- **Vault**: $0.03 per 10,000 operations
- **Secret Storage**: $0.03 per secret per month
- **Monthly Estimate**: <$1/month for typical usage

### Managed Identity Pricing
- **Free** - No additional cost

### Total Estimated Monthly Cost
- **Minimal**: <$5/month for most deployments
- **High Usage** (10,000 validations/day): ~$20/month

**Conclusion**: Cost is negligible compared to value of preventing deployment conflicts

---

## Timeline Summary

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1: Foundation | 2 weeks | Configuration, UI, interfaces |
| Phase 2: Azure Integration | 2 weeks | Authentication, Resource Graph queries |
| Phase 3: Conflict Resolution | 1 week | All resolution strategies |
| Phase 4: API Integration | 1 week | V2 endpoints updated |
| Phase 5: UI Integration | 1 week | Web UI updated |
| Phase 6: Security & Docs | 1 week | Hardening, documentation |
| **Total** | **8 weeks** | **Production-ready feature** |

---

## Alternatives Considered

### Alternative 1: Azure CLI Integration
**Description**: Shell out to `az` CLI instead of SDK  
**Pros**: No additional NuGet packages  
**Cons**: Requires Azure CLI installed, slower, harder to test  
**Decision**: Rejected - SDK is more reliable

### Alternative 2: Azure Resource Manager API (REST)
**Description**: Direct HTTP calls to ARM API  
**Pros**: No SDK dependencies  
**Cons**: Manual auth token management, more code, harder to maintain  
**Decision**: Rejected - SDK provides better abstraction

### Alternative 3: Pre-validation Only (No Auto-increment)
**Description**: Only notify, never auto-increment  
**Pros**: Simpler implementation  
**Cons**: Less helpful for users  
**Decision**: Rejected - Auto-increment is key value-add

### Alternative 4: External Service
**Description**: Separate microservice for validation  
**Pros**: Better separation of concerns  
**Cons**: More complex architecture, deployment overhead  
**Decision**: Rejected - Overkill for this use case

---

## Conclusion

This feature represents a significant enhancement to the Azure Naming Tool, providing users with the ability to ensure their generated names are truly unique within their Azure environment. The implementation is designed to be:

- **Optional**: Users not needing this can ignore it completely
- **Secure**: Multiple authentication methods with industry best practices
- **Flexible**: Configurable conflict resolution strategies
- **Performant**: Caching and batching minimize Azure API overhead
- **User-Friendly**: Clear UI, helpful error messages, comprehensive documentation

**Recommended Next Steps:**
1. Review and approve this plan
2. Gather feedback from key users/stakeholders
3. Prioritize phase 1 for initial prototype
4. Create detailed technical specifications for each phase
5. Begin implementation starting with Phase 1

**Estimated Effort**: 8 weeks (1 developer)  
**Target Release**: v5.1.0  
**Risk Level**: Medium (complex but well-scoped)  
**User Value**: High (prevents deployment conflicts)

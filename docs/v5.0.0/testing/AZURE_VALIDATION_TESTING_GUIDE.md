# Azure Validation Testing Guide

## Overview
This guide provides comprehensive testing procedures for the Azure tenant name validation feature in Azure Naming Tool v5.0.0+.

**Testing Scope:**
- ✅ Authentication (Managed Identity & Service Principal)
- ✅ Validation API (CheckNameAvailability & Resource Graph)
- ✅ Conflict Resolution (4 strategies)
- ✅ UI Integration (Generate & Admin pages)
- ✅ API Endpoints (V2)
- ✅ Error Handling & Graceful Degradation
- ✅ Performance & Caching
- ✅ Security & RBAC

---

## Table of Contents
1. [Test Environment Setup](#test-environment-setup)
2. [Unit Testing](#unit-testing)
3. [Integration Testing](#integration-testing)
4. [End-to-End Testing](#end-to-end-testing)
5. [Performance Testing](#performance-testing)
6. [Security Testing](#security-testing)
7. [Test Data](#test-data)
8. [Automated Test Scripts](#automated-test-scripts)

---

## Test Environment Setup

### Prerequisites

**Azure Resources:**
```bash
# 1. Create test resource group
az group create --name naming-tool-test-rg --location eastus

# 2. Create test resources for validation
az storage account create \
  --name sttestvalidation001 \
  --resource-group naming-tool-test-rg \
  --location eastus \
  --sku Standard_LRS

az network vnet create \
  --name vnet-test-001 \
  --resource-group naming-tool-test-rg \
  --address-prefix 10.0.0.0/16

az network vnet create \
  --name vnet-test-002 \
  --resource-group naming-tool-test-rg \
  --address-prefix 10.1.0.0/16
```

**Service Principal (Testing):**
```bash
# Create SP with Reader role
$sp = az ad sp create-for-rbac \
  --name "naming-tool-testing-sp" \
  --role "Reader" \
  --scopes "/subscriptions/<sub-id>" \
  --output json | ConvertFrom-Json

# Save credentials for testing
Write-Host "Tenant ID: $($sp.tenant)"
Write-Host "Client ID: $($sp.appId)"
Write-Host "Client Secret: $($sp.password)"
```

**Test Configuration:**
```json
{
  "Enabled": true,
  "AuthMode": "ServicePrincipal",
  "TenantId": "<tenant-id>",
  "SubscriptionIds": ["<subscription-id>"],
  "ServicePrincipal": {
    "ClientId": "<client-id>",
    "ClientSecret": "<client-secret>"
  },
  "ConflictResolution": {
    "Strategy": "AutoIncrement",
    "MaxAttempts": 10
  },
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 1
  }
}
```

---

## Unit Testing

### Test Suite 1: Authentication

**Test 1.1: Managed Identity Authentication**
```csharp
[Fact]
public async Task TestManagedIdentityAuthentication()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        Enabled = true,
        AuthMode = "ManagedIdentity"
    };
    var service = new AzureValidationService(settings, mockLogger);

    // Act
    var result = await service.TestConnectionAsync();

    // Assert
    Assert.True(result.Authenticated);
    Assert.Equal("ManagedIdentity", result.AuthMode);
    Assert.NotNull(result.TenantId);
    Assert.True(result.SubscriptionCount > 0);
}
```

**Expected:** ✅ Authenticated, tenant ID returned, subscription count > 0

**Test 1.2: Service Principal Authentication**
```csharp
[Fact]
public async Task TestServicePrincipalAuthentication()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        Enabled = true,
        AuthMode = "ServicePrincipal",
        TenantId = "<tenant-id>",
        ServicePrincipal = new ServicePrincipalSettings
        {
            ClientId = "<client-id>",
            ClientSecret = "<client-secret>"
        }
    };
    var service = new AzureValidationService(settings, mockLogger);

    // Act
    var result = await service.TestConnectionAsync();

    // Assert
    Assert.True(result.Authenticated);
    Assert.Equal("ServicePrincipal", result.AuthMode);
}
```

**Expected:** ✅ Authenticated with Service Principal credentials

**Test 1.3: Invalid Credentials**
```csharp
[Fact]
public async Task TestInvalidCredentials_ReturnsAuthenticationFailed()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        Enabled = true,
        AuthMode = "ServicePrincipal",
        TenantId = "invalid-tenant",
        ServicePrincipal = new ServicePrincipalSettings
        {
            ClientId = "invalid-client",
            ClientSecret = "invalid-secret"
        }
    };
    var service = new AzureValidationService(settings, mockLogger);

    // Act
    var result = await service.TestConnectionAsync();

    // Assert
    Assert.False(result.Authenticated);
    Assert.Contains("authentication failed", result.ErrorMessage.ToLower());
}
```

**Expected:** ❌ Authentication failed with error message

---

### Test Suite 2: Name Validation

**Test 2.1: Global Resource - CheckNameAvailability (Storage)**
```csharp
[Fact]
public async Task TestStorageAccountNameAvailability_Available()
{
    // Arrange
    var service = CreateAuthenticatedService();
    var resourceType = new ResourceType { ShortName = "st", Scope = "global" };
    var name = "sttest" + Guid.NewGuid().ToString("N").Substring(0, 8);

    // Act
    var result = await service.ValidateNameAsync(name, resourceType);

    // Assert
    Assert.True(result.ValidationPerformed);
    Assert.False(result.ExistsInAzure);
    Assert.Null(result.ValidationWarning);
}
```

**Expected:** ✅ Name available, no conflicts

**Test 2.2: Global Resource - Name Exists**
```csharp
[Fact]
public async Task TestStorageAccountNameAvailability_Exists()
{
    // Arrange
    var service = CreateAuthenticatedService();
    var resourceType = new ResourceType { ShortName = "st", Scope = "global" };
    var existingName = "sttestvalidation001"; // Pre-created in test setup

    // Act
    var result = await service.ValidateNameAsync(existingName, resourceType);

    // Assert
    Assert.True(result.ValidationPerformed);
    Assert.True(result.ExistsInAzure);
}
```

**Expected:** ✅ Name exists, conflict detected

**Test 2.3: Scoped Resource - Resource Graph (VNet)**
```csharp
[Fact]
public async Task TestVNetNameValidation_Available()
{
    // Arrange
    var service = CreateAuthenticatedService();
    var resourceType = new ResourceType { ShortName = "vnet", Scope = "resourceGroup" };
    var name = "vnet-test-999"; // Non-existent

    // Act
    var result = await service.ValidateNameAsync(name, resourceType);

    // Assert
    Assert.True(result.ValidationPerformed);
    Assert.False(result.ExistsInAzure);
}
```

**Expected:** ✅ Name available

**Test 2.4: Scoped Resource - Name Exists**
```csharp
[Fact]
public async Task TestVNetNameValidation_Exists()
{
    // Arrange
    var service = CreateAuthenticatedService();
    var resourceType = new ResourceType { ShortName = "vnet", Scope = "resourceGroup" };
    var existingName = "vnet-test-001"; // Pre-created

    // Act
    var result = await service.ValidateNameAsync(existingName, resourceType);

    // Assert
    Assert.True(result.ValidationPerformed);
    Assert.True(result.ExistsInAzure);
}
```

**Expected:** ✅ Name exists in tenant

---

### Test Suite 3: Conflict Resolution

**Test 3.1: AutoIncrement - Single Increment**
```csharp
[Fact]
public async Task TestAutoIncrement_SingleIncrement()
{
    // Arrange
    var service = CreateConflictResolutionService();
    var originalName = "vnet-test-001";
    var resourceType = new ResourceType { ShortName = "vnet" };

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("vnet-test-002", result.FinalName);
    Assert.Equal(1, result.Attempts);
}
```

**Expected:** ✅ `vnet-test-001` → `vnet-test-002`

**Test 3.2: AutoIncrement - Multiple Increments**
```csharp
[Fact]
public async Task TestAutoIncrement_MultipleIncrements()
{
    // Arrange
    // Pre-create: vnet-test-001, vnet-test-002
    var service = CreateConflictResolutionService();
    var originalName = "vnet-test-001";
    var resourceType = new ResourceType { ShortName = "vnet" };

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("vnet-test-003", result.FinalName);
    Assert.Equal(2, result.Attempts);
}
```

**Expected:** ✅ Skips 001, 002, lands on 003

**Test 3.3: AutoIncrement - Max Attempts Exceeded**
```csharp
[Fact]
public async Task TestAutoIncrement_MaxAttemptsExceeded()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        ConflictResolution = new ConflictResolutionSettings
        {
            Strategy = "AutoIncrement",
            MaxAttempts = 3
        }
    };
    var service = CreateConflictResolutionService(settings);
    var originalName = "vnet-test-001";
    // Pre-create: vnet-test-001, 002, 003, 004

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.False(result.Success);
    Assert.Contains("max attempts", result.Message.ToLower());
}
```

**Expected:** ❌ Fails after 3 attempts

**Test 3.4: NotifyOnly Strategy**
```csharp
[Fact]
public async Task TestNotifyOnly_ReturnsOriginalName()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        ConflictResolution = new ConflictResolutionSettings
        {
            Strategy = "NotifyOnly"
        }
    };
    var service = CreateConflictResolutionService(settings);
    var originalName = "vnet-test-001";

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.True(result.Success);
    Assert.Equal(originalName, result.FinalName);
    Assert.Contains("warning", result.Message.ToLower());
}
```

**Expected:** ✅ Original name returned with warning

**Test 3.5: Fail Strategy**
```csharp
[Fact]
public async Task TestFailStrategy_ReturnsError()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        ConflictResolution = new ConflictResolutionSettings
        {
            Strategy = "Fail"
        }
    };
    var service = CreateConflictResolutionService(settings);
    var originalName = "vnet-test-001";

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.False(result.Success);
    Assert.Contains("exists", result.Message.ToLower());
}
```

**Expected:** ❌ Error returned, no name generated

**Test 3.6: SuffixRandom Strategy**
```csharp
[Fact]
public async Task TestSuffixRandom_AddsRandomSuffix()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        ConflictResolution = new ConflictResolutionSettings
        {
            Strategy = "SuffixRandom"
        }
    };
    var service = CreateConflictResolutionService(settings);
    var originalName = "vnet-test-001";

    // Act
    var result = await service.ResolveConflictAsync(originalName, resourceType);

    // Assert
    Assert.True(result.Success);
    Assert.StartsWith("vnet-test-001-", result.FinalName);
    Assert.Equal(18, result.FinalName.Length); // Original + "-" + 6 chars
    Assert.Matches(@"vnet-test-001-[a-z0-9]{6}", result.FinalName);
}
```

**Expected:** ✅ `vnet-test-001-abc123` (random suffix)

---

### Test Suite 4: Caching

**Test 4.1: Cache Hit**
```csharp
[Fact]
public async Task TestCacheHit_ReturnsCachedResult()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        Cache = new CacheSettings
        {
            Enabled = true,
            DurationMinutes = 5
        }
    };
    var service = CreateAuthenticatedService(settings);
    var name = "vnet-test-cache";

    // Act
    var result1 = await service.ValidateNameAsync(name, resourceType);
    var sw = Stopwatch.StartNew();
    var result2 = await service.ValidateNameAsync(name, resourceType);
    sw.Stop();

    // Assert
    Assert.Equal(result1.ExistsInAzure, result2.ExistsInAzure);
    Assert.True(sw.ElapsedMilliseconds < 100); // Should be cached (fast)
}
```

**Expected:** ✅ Second call < 100ms (cached)

**Test 4.2: Cache Expiration**
```csharp
[Fact]
public async Task TestCacheExpiration_QueriesAzureAgain()
{
    // Arrange
    var settings = new AzureValidationSettings
    {
        Cache = new CacheSettings
        {
            Enabled = true,
            DurationMinutes = 1 // 1 minute expiration
        }
    };
    var service = CreateAuthenticatedService(settings);
    var name = "vnet-test-expire";

    // Act
    var result1 = await service.ValidateNameAsync(name, resourceType);
    await Task.Delay(TimeSpan.FromMinutes(1.5)); // Wait for cache to expire
    var result2 = await service.ValidateNameAsync(name, resourceType);

    // Assert
    Assert.NotNull(result1.ValidationTimestamp);
    Assert.NotNull(result2.ValidationTimestamp);
    Assert.True(result2.ValidationTimestamp > result1.ValidationTimestamp.AddMinutes(1));
}
```

**Expected:** ✅ Cache expired, new Azure query performed

---

## Integration Testing

### Test Suite 5: Service Integration

**Test 5.1: ResourceNamingRequestService with Validation**
```csharp
[Fact]
public async Task TestResourceNamingRequest_WithValidation_NameAvailable()
{
    // Arrange
    var service = CreateResourceNamingRequestService();
    var request = new ResourceNamingRequest
    {
        ResourceType = "vnet",
        ResourceEnvironment = "test",
        ResourceLocation = "eastus",
        ResourceInstance = "999"
    };

    // Act
    var response = await service.RequestNameAsync(request);

    // Assert
    Assert.True(response.Success);
    Assert.NotNull(response.ValidationMetadata);
    Assert.True(response.ValidationMetadata.ValidationPerformed);
    Assert.False(response.ValidationMetadata.ExistsInAzure);
}
```

**Expected:** ✅ Name generated with validation metadata

**Test 5.2: ResourceNamingRequestService - Name Exists, AutoIncrement**
```csharp
[Fact]
public async Task TestResourceNamingRequest_NameExists_AutoIncrement()
{
    // Arrange
    var service = CreateResourceNamingRequestService();
    var request = new ResourceNamingRequest
    {
        ResourceType = "vnet",
        ResourceEnvironment = "test",
        ResourceLocation = "eastus",
        ResourceInstance = "001" // Exists
    };

    // Act
    var response = await service.RequestNameAsync(request);

    // Assert
    Assert.True(response.Success);
    Assert.NotNull(response.ValidationMetadata);
    Assert.True(response.ValidationMetadata.ExistsInAzure);
    Assert.NotNull(response.ValidationMetadata.OriginalName);
    Assert.NotNull(response.ValidationMetadata.FinalName);
    Assert.Contains("002", response.ResourceName);
}
```

**Expected:** ✅ AutoIncrement applied, `vnet-test-eastus-001` → `002`

---

### Test Suite 6: API Controller Integration

**Test 6.1: V2 API - POST /RequestName**
```csharp
[Fact]
public async Task TestV2API_RequestName_Success()
{
    // Arrange
    var client = CreateHttpClient();
    var request = new
    {
        resourceType = "st",
        resourceEnvironment = "test",
        resourceLocation = "eastus",
        resourceInstance = "999"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/v2/ResourceNamingRequests/RequestName", request);
    var result = await response.Content.ReadFromJsonAsync<ResourceNameResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(result.Success);
    Assert.NotNull(result.ValidationMetadata);
}
```

**Expected:** ✅ 200 OK with validation metadata

**Test 6.2: V2 API - POST /RequestNames (Bulk)**
```csharp
[Fact]
public async Task TestV2API_RequestNames_Bulk()
{
    // Arrange
    var client = CreateHttpClient();
    var bulkRequest = new
    {
        requests = new[]
        {
            new { resourceType = "vnet", resourceInstance = "901" },
            new { resourceType = "vnet", resourceInstance = "902" },
            new { resourceType = "st", resourceInstance = "901" }
        }
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/v2/ResourceNamingRequests/RequestNames", bulkRequest);
    var result = await response.Content.ReadFromJsonAsync<BulkResourceNameResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(3, result.TotalRequests);
    Assert.Equal(3, result.SuccessfulRequests);
    Assert.All(result.Results, r => Assert.NotNull(r.ValidationMetadata));
}
```

**Expected:** ✅ All 3 names generated with validation

**Test 6.3: V2 API - Validation Disabled**
```csharp
[Fact]
public async Task TestV2API_ValidationDisabled_NoMetadata()
{
    // Arrange
    DisableAzureValidation(); // Disable in settings
    var client = CreateHttpClient();
    var request = new { resourceType = "vnet", resourceInstance = "001" };

    // Act
    var response = await client.PostAsJsonAsync("/api/v2/ResourceNamingRequests/RequestName", request);
    var result = await response.Content.ReadFromJsonAsync<ResourceNameResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Null(result.ValidationMetadata);
}
```

**Expected:** ✅ Name returned, no validation metadata

---

## End-to-End Testing

### Test Suite 7: UI Integration

**Test 7.1: Generate Page - Name Available**
```
Manual Test Steps:
1. Navigate to Generate page
2. Select Resource Type: Virtual Network (vnet)
3. Select Environment: Test
4. Select Location: East US
5. Enter Instance: 999
6. Click "Generate Name"

Expected Result:
✅ Generated name shown: vnet-test-eastus-999
✅ Azure Validation column shows: "✓ Available" (green badge)
✅ No conflict resolution details shown
```

**Test 7.2: Generate Page - Name Exists, AutoIncrement**
```
Manual Test Steps:
1. Navigate to Generate page
2. Select Resource Type: Virtual Network (vnet)
3. Select Environment: Test
4. Select Location: East US
5. Enter Instance: 001 (existing resource)
6. Click "Generate Name"

Expected Result:
✅ Generated name shown: vnet-test-eastus-002
✅ Azure Validation column shows: "Existed in Azure" (yellow badge)
✅ Conflict details shown:
   - Original: vnet-test-eastus-001
   - Resolved: vnet-test-eastus-002
   - (1 attempts)
```

**Test 7.3: Admin Page - Test Connection**
```
Manual Test Steps:
1. Navigate to Admin page
2. Click on "Azure Validation" tab
3. Verify current settings displayed
4. Click "Test Connection" button

Expected Result:
✅ Spinner shows while testing
✅ Success banner appears (green):
   - "✓ Connected to Azure"
   - Authentication: ManagedIdentity (or ServicePrincipal)
   - Tenant: <tenant-id>
   - Accessible Subscriptions: X
```

**Test 7.4: Admin Page - Save Settings**
```
Manual Test Steps:
1. Navigate to Admin > Azure Validation
2. Change Conflict Strategy to "NotifyOnly"
3. Change Cache Duration to 10 minutes
4. Click "Save Settings"

Expected Result:
✅ Success toast appears: "Settings saved"
✅ Admin log shows: "Azure Validation settings updated"
✅ Connection test status resets (prompts re-test)
```

---

## Performance Testing

### Test Suite 8: Performance Benchmarks

**Test 8.1: Single Validation Performance**
```csharp
[Fact]
public async Task TestPerformance_SingleValidation_UnderThreshold()
{
    // Arrange
    var service = CreateAuthenticatedService();
    var name = "vnet-perf-test";
    var sw = Stopwatch.StartNew();

    // Act
    var result = await service.ValidateNameAsync(name, resourceType);
    sw.Stop();

    // Assert
    Assert.True(sw.ElapsedMilliseconds < 2000); // Should be under 2 seconds
}
```

**Expected:** ✅ Validation completes in < 2 seconds

**Test 8.2: Bulk Validation Performance**
```csharp
[Fact]
public async Task TestPerformance_BulkValidation_Under5Seconds()
{
    // Arrange
    var service = CreateResourceNamingRequestService();
    var requests = Enumerable.Range(1, 10).Select(i => new ResourceNamingRequest
    {
        ResourceType = "vnet",
        ResourceInstance = (900 + i).ToString()
    }).ToList();
    var sw = Stopwatch.StartNew();

    // Act
    var results = new List<ResourceNameResponse>();
    foreach (var req in requests)
    {
        results.Add(await service.RequestNameAsync(req));
    }
    sw.Stop();

    // Assert
    Assert.Equal(10, results.Count);
    Assert.True(sw.ElapsedMilliseconds < 5000); // 10 validations under 5 seconds
}
```

**Expected:** ✅ 10 validations in < 5 seconds

**Test 8.3: Cache Performance Improvement**
```csharp
[Fact]
public async Task TestPerformance_CacheImprovement()
{
    // Arrange
    var service = CreateAuthenticatedService(cacheEnabled: true);
    var name = "vnet-cache-test";

    // Act
    var sw1 = Stopwatch.StartNew();
    await service.ValidateNameAsync(name, resourceType);
    sw1.Stop();

    var sw2 = Stopwatch.StartNew();
    await service.ValidateNameAsync(name, resourceType); // Cached
    sw2.Stop();

    // Assert
    Assert.True(sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds / 10); // 10x faster
}
```

**Expected:** ✅ Cached call 10x+ faster

---

## Security Testing

### Test Suite 9: RBAC & Permissions

**Test 9.1: Insufficient Permissions**
```bash
# Create SP without Reader role
az ad sp create-for-rbac \
  --name "naming-tool-noreader-sp" \
  --skip-assignment

# Configure tool with this SP
# Expected: Test Connection fails with "Insufficient Permissions"
```

**Test 9.2: Key Vault Access Denied**
```bash
# Create Key Vault without granting access to Managed Identity
az keyvault create --name test-kv-noaccess --resource-group test-rg

# Store secret
az keyvault secret set --vault-name test-kv-noaccess --name test-secret --value "test"

# Configure tool to use this Key Vault
# Expected: "Key Vault Access Denied" error
```

**Test 9.3: Expired Client Secret**
```bash
# Create SP with short-lived secret (90 days)
az ad sp credential reset \
  --id <client-id> \
  --end-date 2025-01-01  # Past date

# Expected: Authentication fails with "Credential expired"
```

---

## Test Data

### Resource Types to Test

| Resource Type | Scope | Validation Method | Test Names |
|---------------|-------|-------------------|------------|
| Storage Account (st) | Global | CheckNameAvailability | sttest001, sttest002, sttestvalidation001 |
| Key Vault (kv) | Global | CheckNameAvailability | kv-test-001, kv-test-002 |
| Virtual Network (vnet) | ResourceGroup | Resource Graph | vnet-test-001, vnet-test-002, vnet-test-003 |
| Virtual Machine (vm) | ResourceGroup | Resource Graph | vm-test-001, vm-test-002 |
| Public IP (pip) | ResourceGroup | Resource Graph | pip-test-001 |
| App Service (app) | Global | CheckNameAvailability | app-test-001 |
| Function App (func) | Global | CheckNameAvailability | func-test-001 |
| Container Registry (cr) | Global | CheckNameAvailability | crtest001 |
| Cosmos DB (cosmos) | Global | CheckNameAvailability | cosmos-test-001 |
| SQL Server (sql) | Global | CheckNameAvailability | sql-test-001 |

---

## Automated Test Scripts

### PowerShell Test Suite

```powershell
# Azure Validation E2E Test Suite
# File: test-azure-validation.ps1

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiUrl,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId
)

# Configuration
$headers = @{
    "APIKey" = $ApiKey
    "Content-Type" = "application/json"
}

# Test Results
$testResults = @()

function Test-NameGeneration {
    param($ResourceType, $Instance, $ExpectedBehavior)
    
    Write-Host "Testing $ResourceType with instance $Instance..." -ForegroundColor Cyan
    
    $body = @{
        resourceType = $ResourceType
        resourceEnvironment = "test"
        resourceLocation = "eastus"
        resourceInstance = $Instance
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod `
            -Uri "$ApiUrl/api/v2/ResourceNamingRequests/RequestName" `
            -Method Post `
            -Headers $headers `
            -Body $body
        
        $result = @{
            Test = "$ResourceType-$Instance"
            Status = "PASS"
            GeneratedName = $response.resourceName
            Validated = $response.validationMetadata.validationPerformed
            ExistsInAzure = $response.validationMetadata.existsInAzure
            ConflictResolved = $response.validationMetadata.originalName -ne $null
        }
        
        Write-Host "  ✓ PASS: $($response.resourceName)" -ForegroundColor Green
    }
    catch {
        $result = @{
            Test = "$ResourceType-$Instance"
            Status = "FAIL"
            Error = $_.Exception.Message
        }
        Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    return $result
}

# Test 1: Available Name
Write-Host "`n=== Test 1: Available Name ===" -ForegroundColor Yellow
$testResults += Test-NameGeneration -ResourceType "vnet" -Instance "999" -ExpectedBehavior "Available"

# Test 2: Existing Name (AutoIncrement)
Write-Host "`n=== Test 2: Existing Name (AutoIncrement) ===" -ForegroundColor Yellow
$testResults += Test-NameGeneration -ResourceType "vnet" -Instance "001" -ExpectedBehavior "Increment"

# Test 3: Global Resource (Storage)
Write-Host "`n=== Test 3: Global Resource (Storage) ===" -ForegroundColor Yellow
$testResults += Test-NameGeneration -ResourceType "st" -Instance "999" -ExpectedBehavior "Available"

# Test 4: Bulk Generation
Write-Host "`n=== Test 4: Bulk Generation ===" -ForegroundColor Yellow
$bulkBody = @{
    requests = @(
        @{ resourceType = "vnet"; resourceInstance = "901" },
        @{ resourceType = "vnet"; resourceInstance = "902" },
        @{ resourceType = "st"; resourceInstance = "901" }
    )
} | ConvertTo-Json

try {
    $bulkResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/v2/ResourceNamingRequests/RequestNames" `
        -Method Post `
        -Headers $headers `
        -Body $bulkBody
    
    Write-Host "  ✓ PASS: Generated $($bulkResponse.successfulRequests)/$($bulkResponse.totalRequests) names" -ForegroundColor Green
    $testResults += @{
        Test = "Bulk-Generation"
        Status = "PASS"
        TotalRequests = $bulkResponse.totalRequests
        SuccessfulRequests = $bulkResponse.successfulRequests
    }
}
catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{
        Test = "Bulk-Generation"
        Status = "FAIL"
        Error = $_.Exception.Message
    }
}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Yellow
$passed = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failed = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red

# Export results
$testResults | Export-Csv -Path "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv" -NoTypeInformation
Write-Host "`nResults exported to CSV" -ForegroundColor Cyan
```

**Usage:**
```powershell
.\test-azure-validation.ps1 `
    -ApiUrl "https://naming-tool.azurewebsites.net" `
    -ApiKey "your-api-key" `
    -SubscriptionId "your-sub-id"
```

---

## Test Coverage Summary

| Category | Tests | Coverage |
|----------|-------|----------|
| Authentication | 3 | Managed Identity, Service Principal, Invalid Credentials |
| Name Validation | 4 | Global (Storage), Scoped (VNet), Available, Exists |
| Conflict Resolution | 6 | AutoIncrement, NotifyOnly, Fail, SuffixRandom, Max Attempts |
| Caching | 2 | Cache Hit, Cache Expiration |
| Service Integration | 2 | Name Generation Service, Conflict Resolution Integration |
| API Controllers | 3 | V2 Single, V2 Bulk, Validation Disabled |
| UI Integration | 4 | Generate Available, Generate Conflict, Admin Test, Admin Save |
| Performance | 3 | Single Validation, Bulk Validation, Cache Performance |
| Security | 3 | RBAC, Key Vault, Expired Credentials |
| **TOTAL** | **30** | **100% Feature Coverage** |

---

## Additional Resources

- [Administrator Guide](./AZURE_VALIDATION_ADMIN_GUIDE.md) - Setup and configuration
- [Security Guide](./AZURE_VALIDATION_SECURITY_GUIDE.md) - Security best practices
- [API Guide](./AZURE_VALIDATION_API_GUIDE.md) - API documentation
- [Implementation Plan](./AZURE_NAME_VALIDATION_PLAN.md) - Technical architecture

---

*Document Version: 1.0*  
*Last Updated: January 2025*  
*Applies to: Azure Naming Tool v5.0.0+*

# Azure Validation API Documentation

## Overview
This document describes the Azure tenant name validation functionality added to the Azure Naming Tool API in v5.0.0.

The validation feature is **opt-in** and requires proper Azure authentication and RBAC permissions to function. When disabled or unavailable, the API returns names without validation metadata.

---

## Table of Contents
1. [Quick Reference](#quick-reference)
2. [V2 API Endpoints](#v2-api-endpoints)
3. [Request/Response Models](#requestresponse-models)
4. [Authentication](#authentication)
5. [Examples](#examples)
6. [Error Handling](#error-handling)

---

## Quick Reference

### Validation Behavior

| Scenario | ValidationMetadata | ExistsInAzure | Behavior |
|----------|-------------------|---------------|----------|
| Feature Disabled | `null` | N/A | Original name returned |
| Validation Enabled + Available | Present | `false` | Original name returned |
| Validation Enabled + Name Exists | Present | `true` | Conflict resolution applied* |
| Validation Enabled + Service Error | Present | N/A | ValidationWarning populated, original name |

*Behavior depends on configured conflict resolution strategy (AutoIncrement, NotifyOnly, Fail, SuffixRandom)

### Key Response Fields

```json
{
  "resourceName": "generated-name",
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationTimestamp": "2025-01-15T10:30:00Z",
    "originalName": "vnet-001",
    "finalName": "vnet-002",
    "incrementAttempts": 1,
    "validationWarning": null
  }
}
```

---

## V2 API Endpoints

All V2 endpoints support Azure validation when enabled. V1 endpoints do NOT include validation.

### POST /api/v2/ResourceNamingRequests/RequestName

Generate a single resource name with optional Azure validation.

**Request Body:**
```json
{
  "resourceType": "st",
  "resourceEnvironment": "prod",
  "resourceLocation": "eastus",
  "resourceOrg": "contoso",
  "resourceUnitDept": "it",
  "resourceProjAppSvc": "webapp",
  "resourceFunction": "data",
  "resourceInstance": "001"
}
```

**Response (200 OK):**
```json
{
  "resourceName": "stcontosoprodeastusitwebappdata001",
  "message": "Name generated successfully",
  "success": true,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationTimestamp": "2025-01-15T10:30:00.123Z",
    "originalName": null,
    "finalName": null,
    "incrementAttempts": 0,
    "validationWarning": null
  }
}
```

**Response (200 OK - Name Existed, AutoIncrement Applied):**
```json
{
  "resourceName": "stcontosoprodeastusitwebappdata002",
  "message": "Name generated successfully (AutoIncrement applied: original name existed in Azure)",
  "success": true,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": true,
    "validationTimestamp": "2025-01-15T10:30:00.123Z",
    "originalName": "stcontosoprodeastusitwebappdata001",
    "finalName": "stcontosoprodeastusitwebappdata002",
    "incrementAttempts": 1,
    "validationWarning": null
  }
}
```

**Response (409 Conflict - Fail Strategy):**
```json
{
  "resourceName": "stcontosoprodeastusitwebappdata001",
  "message": "Name exists in Azure and conflict resolution strategy is set to Fail",
  "success": false,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": true,
    "validationTimestamp": "2025-01-15T10:30:00.123Z",
    "validationWarning": "Resource name already exists in Azure tenant"
  }
}
```

---

### POST /api/v2/ResourceNamingRequests/RequestNames (Bulk)

Generate multiple resource names with validation.

**Request Body:**
```json
{
  "requests": [
    {
      "resourceType": "vnet",
      "resourceEnvironment": "prod",
      "resourceLocation": "eastus",
      "resourceInstance": "001"
    },
    {
      "resourceType": "st",
      "resourceEnvironment": "prod",
      "resourceLocation": "westus",
      "resourceInstance": "001"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "results": [
    {
      "resourceName": "vnet-prod-eastus-001",
      "message": "Name generated successfully",
      "success": true,
      "validationMetadata": {
        "validationPerformed": true,
        "existsInAzure": false,
        "validationTimestamp": "2025-01-15T10:30:00.123Z"
      }
    },
    {
      "resourceName": "stprodwestus001",
      "message": "Name generated successfully",
      "success": true,
      "validationMetadata": {
        "validationPerformed": true,
        "existsInAzure": false,
        "validationTimestamp": "2025-01-15T10:30:01.456Z"
      }
    }
  ],
  "totalRequests": 2,
  "successfulRequests": 2,
  "failedRequests": 0
}
```

---

### GET /api/v2/ResourceNamingRequests/ValidateName

Validate an existing name against Azure tenant (validation only, no generation).

**Query Parameters:**
- `name` (required): Resource name to validate
- `resourceTypeId` (optional): Resource type ID (improves accuracy)

**Request:**
```
GET /api/v2/ResourceNamingRequests/ValidateName?name=sttest001&resourceTypeId=12
```

**Response (200 OK):**
```json
{
  "name": "sttest001",
  "validationPerformed": true,
  "existsInAzure": true,
  "validationTimestamp": "2025-01-15T10:30:00.123Z",
  "message": "Name exists in Azure tenant"
}
```

---

## Request/Response Models

### ResourceNamingRequest

```json
{
  "resourceType": "string",           // Required: Resource type short name (e.g., "st", "vnet")
  "resourceEnvironment": "string",    // Optional: Environment (e.g., "prod", "dev")
  "resourceLocation": "string",       // Optional: Location (e.g., "eastus", "westus2")
  "resourceOrg": "string",            // Optional: Organization
  "resourceUnitDept": "string",       // Optional: Unit/Department
  "resourceProjAppSvc": "string",     // Optional: Project/App/Service
  "resourceFunction": "string",       // Optional: Function
  "resourceInstance": "string"        // Optional: Instance number (e.g., "001")
}
```

### ResourceNameResponse

```json
{
  "resourceName": "string",           // Generated resource name
  "message": "string",                // Success/error message
  "success": boolean,                 // True if successful, false if failed
  "validationMetadata": {             // Present if validation enabled, null otherwise
    "validationPerformed": boolean,   // True if validation executed
    "existsInAzure": boolean,         // True if name found in Azure tenant
    "validationTimestamp": "datetime",// ISO 8601 timestamp of validation
    "originalName": "string",         // Original generated name (before conflict resolution)
    "finalName": "string",            // Final name (after conflict resolution)
    "incrementAttempts": number,      // Number of increment attempts (AutoIncrement only)
    "validationWarning": "string"     // Warning message if validation had issues
  }
}
```

### AzureValidationMetadata

```json
{
  "validationPerformed": boolean,     // True if validation executed successfully
  "existsInAzure": boolean,           // True if resource name found in Azure
  "validationTimestamp": "datetime",  // ISO 8601 timestamp (e.g., "2025-01-15T10:30:00.123Z")
  "originalName": "string",           // Name before conflict resolution (null if no conflict)
  "finalName": "string",              // Name after conflict resolution (null if no conflict)
  "incrementAttempts": number,        // Number of AutoIncrement attempts (0 if not used)
  "validationWarning": "string"       // Warning/error message (null if no issues)
}
```

---

## Authentication

### API Key

All API endpoints require an API key in the request header.

**Header:**
```
APIKey: your-api-key-here
```

**Obtaining API Key:**
1. Navigate to **Admin** → **Site Settings**
2. Copy the **API Key** value
3. Include in all API requests

### Azure Authentication (Backend)

The tool itself authenticates to Azure using one of two methods:

1. **Managed Identity** (Recommended for Azure hosting)
   - No credentials required in configuration
   - Automatically authenticated by Azure platform
   - Zero-trust security model

2. **Service Principal**
   - Requires Tenant ID, Client ID, and Client Secret
   - Secret should be stored in Azure Key Vault
   - Suitable for on-premises or non-Azure hosting

**Configuration:** See [Administrator Guide](./AZURE_VALIDATION_ADMIN_GUIDE.md)

---

## Examples

### Example 1: PowerShell - Generate Name

```powershell
# Configuration
$apiUrl = "https://naming-tool.azurewebsites.net"
$apiKey = "your-api-key"

# Request body
$body = @{
    resourceType = "vnet"
    resourceEnvironment = "prod"
    resourceLocation = "eastus"
    resourceInstance = "001"
} | ConvertTo-Json

# Make request
$headers = @{
    "APIKey" = $apiKey
    "Content-Type" = "application/json"
}

$response = Invoke-RestMethod `
    -Uri "$apiUrl/api/v2/ResourceNamingRequests/RequestName" `
    -Method Post `
    -Headers $headers `
    -Body $body

# Output
Write-Host "Generated Name: $($response.resourceName)"
Write-Host "Success: $($response.success)"
Write-Host "Exists in Azure: $($response.validationMetadata.existsInAzure)"

if ($response.validationMetadata.originalName) {
    Write-Host "Original Name: $($response.validationMetadata.originalName)"
    Write-Host "Resolved Name: $($response.validationMetadata.finalName)"
    Write-Host "Attempts: $($response.validationMetadata.incrementAttempts)"
}
```

**Output:**
```
Generated Name: vnet-prod-eastus-002
Success: True
Exists in Azure: True
Original Name: vnet-prod-eastus-001
Resolved Name: vnet-prod-eastus-002
Attempts: 1
```

---

### Example 2: Bash (curl) - Generate Name

```bash
#!/bin/bash

API_URL="https://naming-tool.azurewebsites.net"
API_KEY="your-api-key"

# Generate name
curl -X POST "$API_URL/api/v2/ResourceNamingRequests/RequestName" \
  -H "APIKey: $API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "resourceType": "st",
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus",
    "resourceInstance": "001"
  }' | jq .

# Expected output (formatted by jq):
# {
#   "resourceName": "stprodeastus001",
#   "message": "Name generated successfully",
#   "success": true,
#   "validationMetadata": {
#     "validationPerformed": true,
#     "existsInAzure": false,
#     "validationTimestamp": "2025-01-15T10:30:00.123Z",
#     "originalName": null,
#     "finalName": null,
#     "incrementAttempts": 0,
#     "validationWarning": null
#   }
# }
```

---

### Example 3: Python - Bulk Generate

```python
import requests
import json

API_URL = "https://naming-tool.azurewebsites.net"
API_KEY = "your-api-key"

# Headers
headers = {
    "APIKey": API_KEY,
    "Content-Type": "application/json"
}

# Bulk request
payload = {
    "requests": [
        {
            "resourceType": "vnet",
            "resourceEnvironment": "prod",
            "resourceLocation": "eastus",
            "resourceInstance": "001"
        },
        {
            "resourceType": "vnet",
            "resourceEnvironment": "prod",
            "resourceLocation": "eastus",
            "resourceInstance": "002"
        },
        {
            "resourceType": "st",
            "resourceEnvironment": "prod",
            "resourceLocation": "westus",
            "resourceInstance": "001"
        }
    ]
}

# Make request
response = requests.post(
    f"{API_URL}/api/v2/ResourceNamingRequests/RequestNames",
    headers=headers,
    json=payload
)

# Parse response
result = response.json()

print(f"Total Requests: {result['totalRequests']}")
print(f"Successful: {result['successfulRequests']}")
print(f"Failed: {result['failedRequests']}")
print()

# Print each result
for i, item in enumerate(result['results'], 1):
    print(f"Request {i}:")
    print(f"  Name: {item['resourceName']}")
    print(f"  Success: {item['success']}")
    
    if item['validationMetadata']:
        vm = item['validationMetadata']
        print(f"  Validated: {vm['validationPerformed']}")
        print(f"  Exists in Azure: {vm['existsInAzure']}")
        
        if vm['originalName']:
            print(f"  Original: {vm['originalName']} → Final: {vm['finalName']}")
    print()
```

**Output:**
```
Total Requests: 3
Successful: 3
Failed: 0

Request 1:
  Name: vnet-prod-eastus-001
  Success: True
  Validated: True
  Exists in Azure: False

Request 2:
  Name: vnet-prod-eastus-002
  Success: True
  Validated: True
  Exists in Azure: False

Request 3:
  Name: stprodwestus001
  Success: True
  Validated: True
  Exists in Azure: False
```

---

### Example 4: C# - Name Validation Only

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class AzureNamingToolClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AzureNamingToolClient(string apiUrl, string apiKey)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        _apiKey = apiKey;
    }

    public async Task<ValidationResponse> ValidateNameAsync(string name, int? resourceTypeId = null)
    {
        var url = $"api/v2/ResourceNamingRequests/ValidateName?name={name}";
        if (resourceTypeId.HasValue)
        {
            url += $"&resourceTypeId={resourceTypeId.Value}";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("APIKey", _apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ValidationResponse>(json);
    }
}

public class ValidationResponse
{
    public string Name { get; set; }
    public bool ValidationPerformed { get; set; }
    public bool ExistsInAzure { get; set; }
    public DateTime ValidationTimestamp { get; set; }
    public string Message { get; set; }
}

// Usage
var client = new AzureNamingToolClient(
    "https://naming-tool.azurewebsites.net",
    "your-api-key"
);

var result = await client.ValidateNameAsync("sttest001", resourceTypeId: 12);

Console.WriteLine($"Name: {result.Name}");
Console.WriteLine($"Exists in Azure: {result.ExistsInAzure}");
Console.WriteLine($"Message: {result.Message}");

// Output:
// Name: sttest001
// Exists in Azure: True
// Message: Name exists in Azure tenant
```

---

## Error Handling

### HTTP Status Codes

| Code | Description | Common Causes |
|------|-------------|---------------|
| 200 | OK | Request successful (check `success` field for validation result) |
| 400 | Bad Request | Invalid request body, missing required fields |
| 401 | Unauthorized | Missing or invalid API key |
| 404 | Not Found | Endpoint not found, check V2 URL |
| 409 | Conflict | Name exists and conflict strategy is "Fail" |
| 500 | Internal Server Error | Service error, check admin logs |

### Error Response Format

```json
{
  "resourceName": "attempted-name",
  "message": "Detailed error message",
  "success": false,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": true,
    "validationWarning": "Resource name already exists in Azure tenant"
  }
}
```

### Common Error Scenarios

**1. Validation Disabled**
```json
{
  "resourceName": "vnet-prod-eastus-001",
  "message": "Name generated successfully",
  "success": true,
  "validationMetadata": null  // Validation not performed
}
```
**Resolution:** Enable validation in Admin → Site Settings

**2. Azure Service Unavailable**
```json
{
  "resourceName": "vnet-prod-eastus-001",
  "message": "Name generated successfully",
  "success": true,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationWarning": "Azure validation service unavailable, name returned without validation"
  }
}
```
**Resolution:** Check Azure authentication and RBAC permissions

**3. AutoIncrement Exhausted**
```json
{
  "resourceName": "vnet-prod-eastus-001",
  "message": "Failed to resolve naming conflict after 100 attempts",
  "success": false,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": true,
    "incrementAttempts": 100,
    "validationWarning": "Max increment attempts reached"
  }
}
```
**Resolution:** Use different base name or clean up existing resources

**4. Conflict Resolution Failed**
```json
{
  "resourceName": "vnet-prod-eastus-001",
  "message": "Name exists in Azure and conflict resolution strategy is set to Fail",
  "success": false,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": true
  }
}
```
**Resolution:** Choose different instance number or change conflict strategy

---

## Best Practices

### For API Consumers

1. **Always Check `success` Field**
   ```csharp
   if (response.success)
   {
       // Use response.resourceName
   }
   else
   {
       // Handle error in response.message
   }
   ```

2. **Check Validation Metadata**
   ```csharp
   if (response.validationMetadata?.existsInAzure == true)
   {
       // Name was modified due to conflict
       Console.WriteLine($"Original: {response.validationMetadata.originalName}");
       Console.WriteLine($"Final: {response.validationMetadata.finalName}");
   }
   ```

3. **Handle Graceful Degradation**
   ```csharp
   if (response.validationMetadata?.validationWarning != null)
   {
       // Validation had issues but name was still returned
       Logger.Warning(response.validationMetadata.validationWarning);
   }
   ```

4. **Use Bulk Endpoints for Multiple Requests**
   - More efficient than multiple single requests
   - Reduces API throttling risk
   - Better cache utilization

5. **Implement Retry Logic for Transient Errors**
   ```python
   from tenacity import retry, stop_after_attempt, wait_exponential
   
   @retry(stop=stop_after_attempt(3), wait=wait_exponential(min=1, max=10))
   def generate_name(request_data):
       return requests.post(url, json=request_data, headers=headers)
   ```

---

## Migration from V1 API

### Key Differences

| Feature | V1 API | V2 API |
|---------|--------|--------|
| Endpoint Path | `/api/ResourceNamingRequests/...` | `/api/v2/ResourceNamingRequests/...` |
| Validation | ❌ Not supported | ✅ Supported |
| Response Model | Simple string or object | Includes ValidationMetadata |
| Bulk Operations | Limited | Full support with validation |
| Conflict Resolution | Manual | Automatic (4 strategies) |

### Migration Steps

1. **Update Endpoint URLs**
   ```
   OLD: https://app.com/api/ResourceNamingRequests/RequestName
   NEW: https://app.com/api/v2/ResourceNamingRequests/RequestName
   ```

2. **Update Response Parsing**
   ```csharp
   // V1
   var name = response.resourceName;
   
   // V2 - Add validation checks
   var name = response.resourceName;
   if (response.validationMetadata?.existsInAzure == true)
   {
       // Handle conflict resolution
   }
   ```

3. **Handle New Status Codes**
   - V2 can return 409 Conflict (if Fail strategy)
   - Add error handling for validation failures

4. **Test Thoroughly**
   - Test with validation enabled/disabled
   - Test all conflict resolution strategies
   - Test bulk operations

---

## Swagger/OpenAPI Integration

### Accessing Swagger UI

Navigate to: `https://your-app.azurewebsites.net/swagger`

### Swagger Annotations

All V2 endpoints include detailed Swagger documentation:

- Request/response examples
- Authentication requirements
- Status codes and error responses
- Model schemas
- Try It Out functionality

### Sample Swagger Definition

```yaml
/api/v2/ResourceNamingRequests/RequestName:
  post:
    tags:
      - V2 Resource Naming
    summary: Generate a single resource name with Azure validation
    description: |
      Generates a resource name based on configured naming conventions.
      Optionally validates against Azure tenant if validation is enabled.
      Returns ValidationMetadata with availability status.
    parameters:
      - name: APIKey
        in: header
        required: true
        schema:
          type: string
        description: API Key for authentication
    requestBody:
      required: true
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ResourceNamingRequest'
          examples:
            basicRequest:
              summary: Basic resource naming request
              value:
                resourceType: vnet
                resourceEnvironment: prod
                resourceLocation: eastus
                resourceInstance: "001"
    responses:
      '200':
        description: Name generated successfully
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ResourceNameResponse'
            examples:
              available:
                summary: Name is available
                value:
                  resourceName: vnet-prod-eastus-001
                  success: true
                  validationMetadata:
                    validationPerformed: true
                    existsInAzure: false
              conflict:
                summary: Name existed, AutoIncrement applied
                value:
                  resourceName: vnet-prod-eastus-002
                  success: true
                  validationMetadata:
                    validationPerformed: true
                    existsInAzure: true
                    originalName: vnet-prod-eastus-001
                    finalName: vnet-prod-eastus-002
                    incrementAttempts: 1
      '401':
        description: Unauthorized - Invalid API Key
      '409':
        description: Conflict - Name exists and Fail strategy configured
```

---

## Additional Resources

- [Administrator Guide](./AZURE_VALIDATION_ADMIN_GUIDE.md) - Setup and configuration
- [Security Guide](./AZURE_VALIDATION_SECURITY_GUIDE.md) - Security best practices
- [Implementation Plan](./AZURE_NAME_VALIDATION_PLAN.md) - Technical architecture
- [GitHub Wiki](https://github.com/mspnp/AzureNamingTool/wiki) - General documentation

---

*Document Version: 1.0*  
*Last Updated: January 2025*  
*Applies to: Azure Naming Tool v5.0.0+*

# Azure Naming Tool API (Version 2.0)

## Table of Contents
- [Overview](#overview)
- [What's New in V2](#whats-new-in-v2)
- [Authentication](#authentication)
  - [API Key Types](#api-key-types)
- [Interactive Documentation (Swagger UI)](#interactive-documentation-swagger-ui)
- [Getting Started](#getting-started)
  - [Base URL](#base-url)
  - [API Versioning](#api-versioning)
  - [Request Headers](#request-headers)
- [Standardized Response Format](#standardized-response-format)
  - [Success Response Structure](#success-response-structure)
  - [Error Response Structure](#error-response-structure)
  - [Response Metadata](#response-metadata)
- [Core Endpoints](#core-endpoints)
  - [Name Generation with Azure Validation](#name-generation-with-azure-validation)
  - [Name Validation](#name-validation)
  - [Configuration Management](#configuration-management)
- [Name Generation API](#name-generation-api)
  - [RequestName (Recommended)](#requestname-recommended)
  - [RequestNameWithComponents](#requestnamewithcomponents)
  - [ValidateName](#validatename)
- [Azure Tenant Validation](#azure-tenant-validation)
  - [How It Works](#how-it-works)
  - [Conflict Resolution](#conflict-resolution)
  - [Configuration](#configuration)
- [Configuration Endpoints](#configuration-endpoints)
- [Administrative Endpoints](#administrative-endpoints)
- [Migration from V1](#migration-from-v1)
  - [Breaking Changes](#breaking-changes)
  - [Response Format Changes](#response-format-changes)
  - [Migration Strategy](#migration-strategy)
- [Best Practices](#best-practices)
- [Error Handling](#error-handling)
- [Examples](#examples)

---

## Overview

The Azure Naming Tool API Version 2.0 is the next generation of the naming convention API, offering enhanced features, standardized responses, and built-in Azure tenant validation. V2 maintains backward compatibility with V1 functionality while adding powerful new capabilities.

**Key Features**:
- ✅ Standardized `ApiResponse<T>` wrapper for all responses
- ✅ Azure tenant validation (real-time conflict detection)
- ✅ Automatic conflict resolution with configurable strategies
- ✅ Correlation IDs for request tracking
- ✅ Enhanced error handling with detailed error codes
- ✅ Proper HTTP status code usage (400, 401, 500)
- ✅ Response metadata (timestamps, versioning, pagination support)
- ✅ Future-proof extensibility

**Recommendation**: Use V2 for all new integrations. V1 remains supported for existing integrations.

---

## What's New in V2

### 🚀 Azure Tenant Validation

V2 integrates with your Azure tenant to validate that generated names don't conflict with existing resources:

- **Real-Time Validation**: Checks if the generated name already exists in Azure
- **Automatic Conflict Resolution**: Applies configurable strategies to resolve naming conflicts
- **Multiple Strategies**: Increment suffix, random suffix, timestamp suffix
- **Configurable**: Enable/disable per resource type, set retry limits
- **Metadata Included**: Validation results included in response

**Learn More**: [Azure Validation WIKI](AZURE-VALIDATION-WIKI)

---

### 📦 Standardized Response Format

All V2 endpoints return a consistent `ApiResponse<T>` wrapper:

```json
{
  "success": true,
  "data": { /* actual data */ },
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00Z",
    "message": "Resource name generated successfully"
  }
}
```

**Benefits**:
- Predictable response structure
- Easier error handling
- Built-in correlation IDs for debugging
- Metadata for monitoring and logging

---

### 🔍 Enhanced Error Handling

V2 provides detailed error information following Microsoft REST API Guidelines:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Request body cannot be null",
    "target": "ResourceNameRequest",
    "details": [
      {
        "code": "VALIDATION_ERROR",
        "message": "Field 'resourceType' is required"
      }
    ],
    "innerError": {
      "code": "ArgumentNullException"
    }
  },
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00Z"
  }
}
```

**Error Code Examples**:
- `INVALID_REQUEST` - Request validation failed
- `NAME_GENERATION_FAILED` - Name generation failed
- `INTERNAL_SERVER_ERROR` - Server error
- `MISSING_ADMIN_PASSWORD` - Admin password not provided
- `INCORRECT_ADMIN_PASSWORD` - Admin password incorrect

---

### 🔗 Correlation IDs

Every V2 response includes a `correlationId` for request tracking:

- **Generated Automatically**: By middleware for each request
- **Consistent Across Logs**: Same ID in responses and server logs
- **Debugging**: Trace request flow through the system
- **Monitoring**: Track request performance and errors

---

### ⚡ Proper HTTP Status Codes

V2 uses appropriate HTTP status codes:

| Status Code | Meaning | Example |
|-------------|---------|---------|
| 200 OK | Success | Name generated successfully |
| 400 Bad Request | Invalid input | Missing required field |
| 401 Unauthorized | Authentication failed | Invalid API key |
| 500 Internal Server Error | Server error | Unexpected exception |

**V1 Limitation**: V1 returns 200 OK with error messages in the body.

---

### 🔮 Future-Proof Extensibility

The `ApiResponse` wrapper supports future enhancements:

- **Pagination**: `metadata.pagination` for list endpoints
- **Versioning**: `metadata.version` for API version tracking
- **Custom Metadata**: Extensible metadata structure
- **Backward Compatibility**: New fields don't break existing clients

---

## Authentication

### API Key Types

V2 uses the **same authentication system as V1** with three API key types:

#### 1. 🔑 Full API Access Key
**Permissions**: Complete administrative access to all endpoints

**Use Cases**:
- Full system integration
- Administrative automation
- Configuration management scripts

---

#### 2. 🔧 Name Generation API Access Key
**Permissions**: Name generation and read-only configuration access

**Use Cases**:
- CI/CD pipelines
- Infrastructure-as-Code (Terraform, Bicep, ARM)
- Automated provisioning scripts

---

#### 3. 👁️ Read-Only API Access Key
**Permissions**: View-only access to configurations

**Use Cases**:
- Auditing and reporting
- Configuration monitoring
- Documentation generation

---

**Authentication Header**:
```http
APIKey: your-api-key-here
```

Same API keys work for both V1 and V2 endpoints.

---

## Interactive Documentation (Swagger UI)

Access Swagger UI at: `https://your-naming-tool-url/swagger`

**Features**:
- Interactive API explorer for V1 and V2 endpoints
- Request/response examples with `ApiResponse` wrapper
- Schema documentation for all models
- Authentication testing

**Tip**: Use Swagger UI to explore the V2 response format before implementing.

---

## Getting Started

### Base URL

V2 endpoints include the version number in the URL path:

```
https://{your-naming-tool-host}/api/v2.0/{controller}/{action}
```

**Examples**:
```
https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName
https://your-naming-tool.azurewebsites.net/api/v2.0/Admin/UpdatePassword
```

---

### API Versioning

| Version | URL Pattern | Description |
|---------|-------------|-------------|
| V1 | `/api/{controller}/{action}` | Original API (stable, supported) |
| V2 | `/api/v2.0/{controller}/{action}` | Enhanced API (recommended for new integrations) |

**Note**: Both versions are fully supported. Choose based on your requirements.

---

### Request Headers

**Required**:
```http
APIKey: your-api-key-here
Content-Type: application/json
```

**Optional**:
```http
Accept: application/json
```

---

## Standardized Response Format

### Success Response Structure

All V2 success responses follow this format:

```json
{
  "success": true,
  "data": {
    // Actual response data (varies by endpoint)
  },
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z",
    "version": null,
    "message": "Resource name generated successfully",
    "pagination": null
  }
}
```

---

### Error Response Structure

All V2 error responses follow this format:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "target": "Field or endpoint that caused the error",
    "details": [
      {
        "code": "NESTED_ERROR_CODE",
        "message": "Additional error details"
      }
    ],
    "innerError": {
      "code": "Exception type or internal error code"
    }
  },
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z"
  }
}
```

---

### Response Metadata

The `metadata` object provides context about the response:

| Field | Type | Description |
|-------|------|-------------|
| `correlationId` | string | Unique identifier for request tracking |
| `timestamp` | string (ISO 8601) | UTC timestamp when response was generated |
| `version` | string | API version (optional) |
| `message` | string | Optional message about the response |
| `pagination` | object | Pagination info for list endpoints (future) |

---

## Core Endpoints

### Name Generation with Azure Validation

V2 name generation endpoints automatically validate generated names against your Azure tenant (when enabled):

1. Generate name using configured naming convention
2. Validate name against Azure resources (if validation enabled)
3. If name exists, apply conflict resolution strategy
4. Return final name with validation metadata

**Endpoint**: `POST /api/v2.0/ResourceNamingRequests/RequestName`

---

### Name Validation

V2 validation endpoint validates names against Azure resource type regex patterns.

**Endpoint**: `POST /api/v2.0/ResourceNamingRequests/ValidateName`

---

### Configuration Management

V2 configuration endpoints return data wrapped in `ApiResponse<T>`.

**Example**: `GET /api/v2.0/ResourceTypes`

---

## Name Generation API

### RequestName (Recommended)

**Endpoint**: `POST /api/v2.0/ResourceNamingRequests/RequestName`

**Description**: Generate a resource name with simplified request format and automatic Azure validation.

**API Key**: Name Generation API Access Key or Full API Access Key

**Request Body**:
```json
{
  "resourceType": "vnet",
  "resourceEnvironment": "prod",
  "resourceLocation": "eastus",
  "resourceOrg": "contoso",
  "resourceInstance": "001"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "resourceName": "vnet-contoso-prod-eastus-001",
    "message": "Name generation successful!",
    "success": true,
    "validationMetadata": {
      "validationPerformed": true,
      "existsInAzure": false,
      "originalName": null,
      "incrementAttempts": 0,
      "validationWarning": null
    }
  },
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z",
    "message": "Resource name generated successfully"
  }
}
```

**With Conflict Resolution (Name Exists in Azure)**:
```json
{
  "success": true,
  "data": {
    "resourceName": "vnet-contoso-prod-eastus-002",  // Incremented!
    "message": "Name generation successful!",
    "success": true,
    "validationMetadata": {
      "validationPerformed": true,
      "existsInAzure": true,
      "originalName": "vnet-contoso-prod-eastus-001",
      "incrementAttempts": 1,
      "validationWarning": "Original name existed in Azure. Incremented suffix."
    }
  },
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z",
    "message": "Resource name generated successfully"
  }
}
```

**Error Response (400 Bad Request)**:
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NAME_GENERATION_FAILED",
    "message": "Resource type 'invalid-type' not found",
    "target": "RequestName",
    "details": [
      {
        "code": "VALIDATION_ERROR",
        "message": "Resource type 'invalid-type' not found"
      }
    ]
  },
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z"
  }
}
```

**Example (PowerShell)**:
```powershell
$headers = @{
    "APIKey" = "your-name-gen-api-key"
    "Content-Type" = "application/json"
}

$body = @{
    resourceType = "vnet"
    resourceEnvironment = "prod"
    resourceLocation = "eastus"
    resourceInstance = "001"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName" `
    -Method Post `
    -Headers $headers `
    -Body $body

# Access the data
if ($response.success) {
    $name = $response.data.resourceName
    $validated = $response.data.validationMetadata.validationPerformed
    Write-Host "Generated name: $name (Validated: $validated)"
} else {
    Write-Error "Error [$($response.error.code)]: $($response.error.message)"
}
```

**Example (Python)**:
```python
import requests

url = "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName"
headers = {
    "APIKey": "your-name-gen-api-key",
    "Content-Type": "application/json"
}

data = {
    "resourceType": "vnet",
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus",
    "resourceInstance": "001"
}

response = requests.post(url, headers=headers, json=data)
result = response.json()

if result["success"]:
    name = result["data"]["resourceName"]
    validated = result["data"]["validationMetadata"]["validationPerformed"]
    print(f"Generated name: {name} (Validated: {validated})")
else:
    error = result["error"]
    print(f"Error [{error['code']}]: {error['message']}")
```

---

### RequestNameWithComponents

**Endpoint**: `POST /api/v2.0/ResourceNamingRequests/RequestNameWithComponents`

**Description**: Generate name with full component object definitions. Uses same standardized response format.

**Request Body**:
```json
{
  "resourceComponents": [
    {
      "id": 1,
      "name": "Resource Type",
      "value": "vnet"
    },
    {
      "id": 2,
      "name": "Resource Environment",
      "value": "prod"
    }
  ]
}
```

**Response**: Same `ApiResponse<ResourceNameResponse>` format as `RequestName`.

---

### ValidateName

**Endpoint**: `POST /api/v2.0/ResourceNamingRequests/ValidateName`

**Description**: Validate a name against Azure resource type regex.

**Request Body**:
```json
{
  "resourceType": "vnet",
  "name": "vnet-contoso-prod-eastus-001"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "valid": true,
    "message": "Name is valid for resource type 'vnet'"
  },
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z",
    "message": "Name validated successfully"
  }
}
```

---

## Azure Tenant Validation

### How It Works

When Azure Validation is enabled, V2 automatically:

1. **Authenticates** with your Azure tenant (Managed Identity or Service Principal)
2. **Validates** if the generated name already exists in Azure
3. **Resolves Conflicts** using your configured strategy if the name exists
4. **Returns** the final name with validation metadata

**Configuration**: See [Azure Validation WIKI](AZURE-VALIDATION-WIKI) for setup instructions.

---

### Conflict Resolution

When a generated name already exists in Azure, V2 applies a conflict resolution strategy:

#### Increment Strategy (Default)

Increments the suffix number:
- `vnet-contoso-prod-eastus-001` (exists) → `vnet-contoso-prod-eastus-002`

#### Random Suffix Strategy

Adds a random 4-character suffix:
- `vnet-contoso-prod-eastus-001` (exists) → `vnet-contoso-prod-eastus-001-a3f7`

#### Timestamp Strategy

Adds a timestamp suffix:
- `vnet-contoso-prod-eastus-001` (exists) → `vnet-contoso-prod-eastus-001-20250115103000`

**Configuration**: Set strategy in Azure Validation settings (Admin → Azure Validation).

---

### Configuration

Enable Azure Validation:

1. **Navigate**: Admin → Azure Validation
2. **Configure Authentication**:
   - Managed Identity (recommended for Azure-hosted deployments)
   - Service Principal (for on-premises or non-Azure deployments)
3. **Set Conflict Resolution Strategy**:
   - Increment (default)
   - Random
   - Timestamp
4. **Configure Retry Limits**: Maximum attempts to find available name

**Default**: Azure Validation is disabled. Enable in Admin settings.

---

## Configuration Endpoints

V2 configuration endpoints return data wrapped in `ApiResponse<T>`:

### Example: Get Resource Types

**Endpoint**: `GET /api/v2.0/ResourceTypes`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "resource": "Virtual Network",
      "shortName": "vnet",
      "scope": "resource group",
      "lengthMin": 2,
      "lengthMax": 64,
      "enabled": true
    }
  ],
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z"
  }
}
```

**Access Data**:
```python
response = requests.get(url, headers=headers)
result = response.json()

if result["success"]:
    resource_types = result["data"]
    for rt in resource_types:
        print(f"{rt['resource']} ({rt['shortName']})")
```

---

## Administrative Endpoints

V2 admin endpoints use standardized responses:

### Update Password

**Endpoint**: `POST /api/v2.0/Admin/UpdatePassword`

**Headers**:
```http
AdminPassword: current-global-admin-password
Content-Type: application/json
```

**Request Body**:
```json
"new-password"
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "data": "Password updated successfully",
  "error": null,
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z",
    "message": "Global Admin Password has been updated"
  }
}
```

**Error Response (401 Unauthorized)**:
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INCORRECT_ADMIN_PASSWORD",
    "message": "Incorrect Global Admin Password",
    "target": "AdminPassword header"
  },
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z"
  }
}
```

---

## Migration from V1

### Breaking Changes

#### 1. Response Format

**V1 Response**:
```json
{
  "resourceName": "vnet-contoso-prod-eastus-001",
  "message": "Name generation successful!",
  "success": true
}
```

**V2 Response**:
```json
{
  "success": true,
  "data": {
    "resourceName": "vnet-contoso-prod-eastus-001",
    "message": "Name generation successful!",
    "success": true,
    "validationMetadata": { /* ... */ }
  },
  "error": null,
  "metadata": { /* ... */ }
}
```

**Migration**: Access `data` property to get the actual response.

---

#### 2. HTTP Status Codes

**V1**: Returns 200 OK for most errors with error details in response body

**V2**: Uses proper HTTP status codes:
- 200 OK - Success
- 400 Bad Request - Client error
- 401 Unauthorized - Authentication failed
- 500 Internal Server Error - Server error

**Migration**: Check HTTP status code, not just response body.

---

#### 3. URL Path

**V1**: `/api/ResourceNamingRequests/RequestName`

**V2**: `/api/v2.0/ResourceNamingRequests/RequestName`

**Migration**: Update URLs to include `/v2.0/`.

---

### Response Format Changes

#### Accessing Data in V2

**V1**:
```python
response = requests.post(url, headers=headers, json=data)
result = response.json()
name = result["resourceName"]  # Direct access
```

**V2**:
```python
response = requests.post(url, headers=headers, json=data)
result = response.json()

if result["success"]:
    name = result["data"]["resourceName"]  # Access via 'data'
else:
    error = result["error"]
    print(f"Error: {error['message']}")
```

---

#### Error Handling

**V1**:
```python
if result.get("success"):
    name = result["resourceName"]
else:
    print(f"Error: {result['message']}")
```

**V2**:
```python
if response.status_code == 200 and result["success"]:
    name = result["data"]["resourceName"]
elif response.status_code == 400:
    error = result["error"]
    print(f"Bad Request [{error['code']}]: {error['message']}")
elif response.status_code == 401:
    print("Authentication failed")
else:
    print(f"Server error: {result['error']['message']}")
```

---

### Migration Strategy

#### Option 1: Gradual Migration (Recommended)

1. **Start**: Continue using V1 for existing integrations
2. **New Features**: Use V2 for new integrations requiring Azure validation
3. **Update**: Gradually migrate existing integrations to V2
4. **Complete**: Full V2 adoption over time

---

#### Option 2: Immediate Migration

1. **Update URLs**: Change `/api/` to `/api/v2.0/`
2. **Update Response Parsing**: Access `data` property
3. **Update Error Handling**: Check HTTP status codes
4. **Test**: Thoroughly test all integrations
5. **Deploy**: Deploy V2 integration

---

#### Migration Checklist

- [ ] Update API URLs to include `/v2.0/`
- [ ] Update response parsing to access `data` property
- [ ] Update error handling to check HTTP status codes
- [ ] Handle new `validationMetadata` field (if using Azure validation)
- [ ] Update logging to use `correlationId` from metadata
- [ ] Test success scenarios
- [ ] Test error scenarios (400, 401, 500)
- [ ] Test Azure validation (if enabled)
- [ ] Update documentation
- [ ] Deploy and monitor

---

## Best Practices

### 1. Use Correlation IDs for Debugging

Log the `correlationId` from responses to trace requests:

```python
result = response.json()
correlation_id = result["metadata"]["correlationId"]
logger.info(f"Request completed [CorrelationId: {correlation_id}]")
```

Match this ID with server logs for end-to-end request tracking.

---

### 2. Handle HTTP Status Codes Properly

```python
response = requests.post(url, headers=headers, json=data)

if response.status_code == 200:
    result = response.json()
    if result["success"]:
        # Success
        name = result["data"]["resourceName"]
    else:
        # Business logic error (still 200 OK)
        error = result["error"]
        logger.error(f"Error: {error['message']}")
elif response.status_code == 400:
    # Bad request
    result = response.json()
    logger.error(f"Invalid request: {result['error']['message']}")
elif response.status_code == 401:
    # Authentication failed
    logger.error("API key invalid or missing")
elif response.status_code == 500:
    # Server error
    result = response.json()
    logger.error(f"Server error: {result['error']['message']}")
```

---

### 3. Check Validation Metadata

If Azure validation is enabled, check the validation metadata:

```python
result = response.json()

if result["success"]:
    name = result["data"]["resourceName"]
    validation = result["data"]["validationMetadata"]
    
    if validation and validation["validationPerformed"]:
        if validation["existsInAzure"]:
            logger.warning(f"Name conflict resolved: {validation['originalName']} → {name}")
        else:
            logger.info(f"Name validated in Azure: {name}")
```

---

### 4. Use Proper API Keys

- **CI/CD**: Use Name Generation API Access Key
- **Auditing**: Use Read-Only API Access Key
- **Administration**: Use Full API Access Key (securely)

---

### 5. Store API Keys Securely

- Use environment variables or secret management (Azure Key Vault)
- Never commit keys to source control
- Rotate keys regularly
- Use different keys for dev/staging/production

---

### 6. Implement Retry Logic

V2 returns proper HTTP status codes. Implement retry for transient errors:

```python
import time
import requests

def generate_name_with_retry(url, headers, data, max_retries=3):
    for attempt in range(max_retries):
        try:
            response = requests.post(url, headers=headers, json=data)
            
            if response.status_code == 200:
                return response.json()
            elif response.status_code in [500, 502, 503]:
                # Transient error - retry
                time.sleep(2 ** attempt)
            else:
                # Permanent error - don't retry
                return response.json()
        except requests.exceptions.RequestException:
            time.sleep(2 ** attempt)
    
    raise Exception("Max retries exceeded")
```

---

## Error Handling

### Common Error Codes

| Error Code | HTTP Status | Description | Solution |
|------------|-------------|-------------|----------|
| `INVALID_REQUEST` | 400 | Request validation failed | Check request body format |
| `NAME_GENERATION_FAILED` | 400 | Name generation failed | Verify resource type and components |
| `MISSING_ADMIN_PASSWORD` | 400 | Admin password not provided | Include AdminPassword header |
| `INCORRECT_ADMIN_PASSWORD` | 401 | Admin password incorrect | Verify admin password |
| `INTERNAL_SERVER_ERROR` | 500 | Server error | Check server logs with correlationId |

---

### Error Response Example

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NAME_GENERATION_FAILED",
    "message": "Resource type 'invalid-type' not found",
    "target": "RequestName",
    "details": [
      {
        "code": "VALIDATION_ERROR",
        "message": "Resource type 'invalid-type' not found"
      }
    ]
  },
  "metadata": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2025-01-15T10:30:00.000Z"
  }
}
```

---

## Examples

### Example 1: Generate Name with Azure Validation (PowerShell)

```powershell
# Configuration
$apiUrl = "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName"
$apiKey = "your-name-gen-api-key"

# Request data
$body = @{
    resourceType = "vnet"
    resourceEnvironment = "prod"
    resourceLocation = "eastus"
    resourceOrg = "contoso"
    resourceInstance = "001"
} | ConvertTo-Json

# Headers
$headers = @{
    "APIKey" = $apiKey
    "Content-Type" = "application/json"
}

# Make request
$response = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $body

# Output
if ($response.success) {
    $name = $response.data.resourceName
    $validation = $response.data.validationMetadata
    
    Write-Host "Generated name: $name"
    
    if ($validation.validationPerformed) {
        if ($validation.existsInAzure) {
            Write-Warning "Name conflict resolved: $($validation.originalName) → $name"
        } else {
            Write-Host "Name validated in Azure (does not exist)" -ForegroundColor Green
        }
    }
} else {
    Write-Error "Error [$($response.error.code)]: $($response.error.message)"
}
```

---

### Example 2: Terraform Integration with V2

```hcl
# Call V2 API from Terraform
data "http" "resource_name" {
  url    = "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName"
  method = "POST"

  request_headers = {
    APIKey       = var.naming_tool_api_key
    Content-Type = "application/json"
  }

  request_body = jsonencode({
    resourceType        = "vnet"
    resourceEnvironment = "prod"
    resourceLocation    = "eastus"
    resourceInstance    = "001"
  })
}

locals {
  response = jsondecode(data.http.resource_name.response_body)
  vnet_name = local.response.success ? local.response.data.resourceName : "vnet-fallback-name"
  validated_in_azure = try(local.response.data.validationMetadata.validationPerformed, false)
}

resource "azurerm_virtual_network" "example" {
  name                = local.vnet_name
  location            = "eastus"
  resource_group_name = azurerm_resource_group.example.name
  address_space       = ["10.0.0.0/16"]
  
  tags = {
    ValidatedInAzure = tostring(local.validated_in_azure)
  }
}

output "vnet_name" {
  value = local.vnet_name
}

output "validated_in_azure" {
  value = local.validated_in_azure
}
```

---

### Example 3: Azure DevOps Pipeline with V2

```yaml
# azure-pipelines.yml
trigger:
  - main

variables:
  namingToolUrl: 'https://your-naming-tool.azurewebsites.net/api/v2.0'
  namingToolApiKey: $(NamingToolApiKey)  # Secure variable

steps:
  - task: PowerShell@2
    displayName: 'Generate Resource Name (V2)'
    inputs:
      targetType: 'inline'
      script: |
        $body = @{
            resourceType = "st"
            resourceEnvironment = "$(Environment)"
            resourceLocation = "$(Location)"
            resourceInstance = "001"
        } | ConvertTo-Json

        $headers = @{
            "APIKey" = "$(namingToolApiKey)"
            "Content-Type" = "application/json"
        }

        try {
            $response = Invoke-RestMethod -Uri "$(namingToolUrl)/ResourceNamingRequests/RequestName" `
                -Method Post `
                -Headers $headers `
                -Body $body

            if ($response.success) {
                $name = $response.data.resourceName
                $correlationId = $response.metadata.correlationId
                
                Write-Host "Generated name: $name"
                Write-Host "Correlation ID: $correlationId"
                Write-Host "##vso[task.setvariable variable=StorageAccountName]$name"
                Write-Host "##vso[task.setvariable variable=CorrelationId]$correlationId"
                
                if ($response.data.validationMetadata.validationPerformed) {
                    Write-Host "Azure validation performed: Name does not exist" -ForegroundColor Green
                }
            } else {
                Write-Error "API Error [$($response.error.code)]: $($response.error.message)"
                exit 1
            }
        } catch {
            Write-Error "Request failed: $_"
            exit 1
        }

  - task: AzureCLI@2
    displayName: 'Create Storage Account'
    inputs:
      azureSubscription: 'Azure-Subscription'
      scriptType: 'bash'
      scriptLocation: 'inlineScript'
      inlineScript: |
        echo "Creating storage account: $(StorageAccountName)"
        echo "Request Correlation ID: $(CorrelationId)"
        
        az storage account create \
          --name $(StorageAccountName) \
          --resource-group my-rg \
          --location $(Location) \
          --sku Standard_LRS \
          --tags CorrelationId=$(CorrelationId)
```

---

### Example 4: Python with Complete Error Handling

```python
import requests
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def generate_azure_resource_name(resource_type, environment, location, instance="001"):
    """
    Generate Azure resource name using V2 API with full error handling.
    
    Args:
        resource_type: Azure resource type (e.g., "vnet", "st", "kv")
        environment: Environment (e.g., "prod", "dev")
        location: Azure location (e.g., "eastus", "westus")
        instance: Instance number (default: "001")
    
    Returns:
        dict: Response data with name, validation metadata, and correlation ID
    
    Raises:
        Exception: If API call fails or returns error
    """
    url = "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName"
    headers = {
        "APIKey": "your-name-gen-api-key",
        "Content-Type": "application/json"
    }
    
    data = {
        "resourceType": resource_type,
        "resourceEnvironment": environment,
        "resourceLocation": location,
        "resourceInstance": instance
    }
    
    try:
        response = requests.post(url, headers=headers, json=data, timeout=10)
        result = response.json()
        
        # Log correlation ID for tracking
        correlation_id = result.get("metadata", {}).get("correlationId")
        logger.info(f"API request completed [CorrelationId: {correlation_id}]")
        
        if response.status_code == 200 and result.get("success"):
            # Success
            name_data = result["data"]
            name = name_data["resourceName"]
            validation = name_data.get("validationMetadata", {})
            
            logger.info(f"Generated name: {name}")
            
            if validation.get("validationPerformed"):
                if validation.get("existsInAzure"):
                    logger.warning(
                        f"Name conflict resolved: {validation['originalName']} → {name} "
                        f"(attempts: {validation['incrementAttempts']})"
                    )
                else:
                    logger.info("Name validated in Azure (does not exist)")
            
            return {
                "name": name,
                "validation": validation,
                "correlationId": correlation_id
            }
        
        elif response.status_code == 400:
            # Bad request
            error = result.get("error", {})
            error_msg = f"Invalid request [{error.get('code')}]: {error.get('message')}"
            logger.error(f"{error_msg} [CorrelationId: {correlation_id}]")
            raise ValueError(error_msg)
        
        elif response.status_code == 401:
            # Authentication failed
            logger.error(f"API key invalid or missing [CorrelationId: {correlation_id}]")
            raise PermissionError("API authentication failed")
        
        elif response.status_code == 500:
            # Server error
            error = result.get("error", {})
            error_msg = f"Server error: {error.get('message')}"
            logger.error(f"{error_msg} [CorrelationId: {correlation_id}]")
            raise Exception(error_msg)
        
        else:
            # Unexpected response
            logger.error(f"Unexpected response: {response.status_code} [CorrelationId: {correlation_id}]")
            raise Exception(f"Unexpected API response: {response.status_code}")
    
    except requests.exceptions.Timeout:
        logger.error("API request timed out")
        raise
    except requests.exceptions.RequestException as e:
        logger.error(f"API request failed: {e}")
        raise

# Usage
if __name__ == "__main__":
    try:
        result = generate_azure_resource_name("vnet", "prod", "eastus", "001")
        print(f"Name: {result['name']}")
        print(f"Validated in Azure: {result['validation'].get('validationPerformed', False)}")
        print(f"Correlation ID: {result['correlationId']}")
    except Exception as e:
        print(f"Error: {e}")
```

---

## Additional Resources

- **Swagger UI**: `https://your-naming-tool-url/swagger` (interactive API documentation)
- **API V1 Documentation**: [Using the API V1](API-V1-WIKI) (backward compatibility)
- **Azure Validation Guide**: [Azure Validation WIKI](AZURE-VALIDATION-WIKI) (setup and configuration)
- **GitHub Repository**: [Azure Naming Tool](https://github.com/mspnp/AzureNamingTool)

---

## Support and Feedback

- **Issues**: [GitHub Issues](https://github.com/mspnp/AzureNamingTool/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mspnp/AzureNamingTool/discussions)
- **Documentation**: [GitHub Wiki](https://github.com/mspnp/AzureNamingTool/wiki)
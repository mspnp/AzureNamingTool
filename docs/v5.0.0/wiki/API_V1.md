# Azure Naming Tool API (Version 1.0)

## Table of Contents
- [Overview](#overview)
- [Authentication](#authentication)
  - [API Key Types](#api-key-types)
  - [API Key Management](#api-key-management)
- [Interactive Documentation (Swagger UI)](#interactive-documentation-swagger-ui)
- [Getting Started](#getting-started)
  - [Base URL](#base-url)
  - [API Versioning](#api-versioning)
  - [Request Headers](#request-headers)
- [Core Endpoints](#core-endpoints)
  - [Name Generation](#name-generation)
  - [Name Validation](#name-validation)
  - [Configuration Management](#configuration-management)
- [Name Generation API](#name-generation-api)
  - [RequestName (Recommended)](#requestname-recommended)
  - [RequestNameWithComponents](#requestnamewithcomponents)
  - [ValidateName](#validatename)
- [Configuration Endpoints](#configuration-endpoints)
  - [Resource Types](#resource-types)
  - [Resource Locations](#resource-locations)
  - [Resource Environments](#resource-environments)
  - [Resource Organizations](#resource-organizations)
  - [Resource Unit/Departments](#resource-unitdepartments)
  - [Resource Functions](#resource-functions)
  - [Resource Project/App/Services](#resource-projectappservices)
  - [Custom Components](#custom-components)
  - [Resource Delimiters](#resource-delimiters)
- [Administrative Endpoints](#administrative-endpoints)
  - [Admin Settings](#admin-settings)
  - [Generated Names Log](#generated-names-log)
  - [Import/Export](#importexport)
- [Response Formats](#response-formats)
  - [Success Responses](#success-responses)
  - [Error Responses](#error-responses)
- [Best Practices](#best-practices)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Migration to V2](#migration-to-v2)
- [Examples](#examples)

---

## Overview

The Azure Naming Tool API Version 1.0 provides programmatic access to generate standardized Azure resource names based on your organization's naming conventions. The API allows you to:

- Generate resource names using configured naming patterns
- Validate resource names against Azure naming rules
- Manage naming convention components (environments, locations, resource types, etc.)
- Export and import configurations
- Access naming history and logs

The V1 API is production-ready and stable. For new integrations, consider using [API V2](API-V2-WIKI) which offers enhanced features, standardized responses, and Azure tenant validation.

---

## Authentication

### API Key Types

The Azure Naming Tool uses API key authentication with **three distinct key types**, each providing different levels of access:

#### 1. 🔑 Full API Access Key
**Purpose**: Complete administrative access to all API endpoints

**Permissions**:
- ✅ All GET endpoints (read access)
- ✅ All POST endpoints (write access)
- ✅ Name generation endpoints
- ✅ Configuration management (create, update, delete)
- ✅ Administrative functions (password changes, key management)
- ✅ Import/Export operations

**Use Cases**:
- Full system integration
- Administrative automation
- Configuration management scripts
- Complete CI/CD pipeline integration

**Security Note**: This key provides unrestricted access. Store securely and rotate regularly.

---

#### 2. 🔧 Name Generation API Access Key
**Purpose**: Limited access for name generation only

**Permissions**:
- ✅ Name generation endpoints (`RequestName`, `RequestNameWithComponents`)
- ✅ Name validation endpoint (`ValidateName`)
- ✅ Read-only access to configuration endpoints (GET requests)
- ❌ Configuration modifications (POST to config endpoints)
- ❌ Administrative functions

**Use Cases**:
- CI/CD pipelines (generate names during deployments)
- Infrastructure-as-Code (IaC) tools (Terraform, Bicep, ARM templates)
- Automated provisioning scripts
- Developer tools and utilities

**Security Note**: Recommended for most automation scenarios. Prevents accidental configuration changes.

---

#### 3. 👁️ Read-Only API Access Key
**Purpose**: View-only access to configurations

**Permissions**:
- ✅ All GET endpoints (read configuration data)
- ❌ Name generation endpoints
- ❌ All POST endpoints (no write access)
- ❌ Configuration modifications
- ❌ Administrative functions

**Use Cases**:
- Auditing and reporting
- Configuration monitoring
- Documentation generation
- External integrations requiring configuration visibility

**Security Note**: Safest key type for third-party integrations and read-only access scenarios.

---

### API Key Management

#### Obtaining API Keys

1. **Access the Admin Portal**
   - Navigate to the Azure Naming Tool web interface
   - Log in with the Global Admin Password

2. **Generate/View API Keys**
   - Go to **Configuration** → **Admin**
   - View or generate the three API key types
   - Copy and securely store the keys

#### Using API Keys in Requests

Include the API key in the `APIKey` header for all requests:

```http
GET /api/ResourceTypes HTTP/1.1
Host: your-naming-tool.azurewebsites.net
APIKey: your-api-key-here
Content-Type: application/json
```

#### Updating API Keys

Use the Admin endpoints to update keys (requires Global Admin Password):

```http
POST /api/Admin/UpdateAPIKey HTTP/1.1
Host: your-naming-tool.azurewebsites.net
AdminPassword: your-global-admin-password
Content-Type: application/json

"new-api-key-value"
```

---

## Interactive Documentation (Swagger UI)

The Azure Naming Tool includes **Swagger UI** for interactive API documentation and testing.

### Accessing Swagger UI

Navigate to: `https://your-naming-tool-url/swagger`

### Features

- **Interactive API Explorer**: Test all endpoints directly from the browser
- **Request/Response Examples**: View sample payloads and responses
- **Schema Documentation**: Explore request and response models
- **Authentication Testing**: Test different API keys and permissions
- **Response Codes**: View all possible HTTP status codes for each endpoint

### Using Swagger UI

1. **Authorize**: Click the "Authorize" button and enter your API key
2. **Explore Endpoints**: Browse endpoints by category
3. **Try It Out**: Click "Try it out" on any endpoint
4. **Execute**: Fill in parameters and click "Execute"
5. **View Response**: See the actual API response and status code

**Tip**: Use Swagger UI to understand request/response formats before implementing API calls in your code.

---

## Getting Started

### Base URL

Format: `https://{your-naming-tool-host}/api`

Examples:
- Azure App Service: `https://your-naming-tool.azurewebsites.net/api`
- Docker: `http://localhost:8081/api`
- Custom Domain: `https://naming.yourdomain.com/api`

### API Versioning

V1 endpoints do NOT include a version number in the URL path:

```
https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName
```

For V2 endpoints, see the [API V2 documentation](API-V2-WIKI).

### Request Headers

**Required Headers**:
```http
APIKey: your-api-key-here
Content-Type: application/json
```

**Optional Headers**:
```http
Accept: application/json
```

---

## Core Endpoints

### Name Generation

| Endpoint | Method | Description | API Key Required |
|----------|--------|-------------|------------------|
| `/api/ResourceNamingRequests/RequestName` | POST | Generate name with simple format (recommended) | Name Generation or Full Access |
| `/api/ResourceNamingRequests/RequestNameWithComponents` | POST | Generate name with full component definition | Name Generation or Full Access |
| `/api/ResourceNamingRequests/ValidateName` | POST | Validate a name against resource type regex | Name Generation or Full Access |

### Name Validation

The `ValidateName` endpoint validates names using Azure resource type regex patterns (NOT the tool's configuration).

### Configuration Management

All configuration endpoints support:
- **GET** (list all items): Requires any API key
- **POST** (create/update): Requires Full Access API key

Configuration categories:
- Resource Types
- Resource Locations
- Resource Environments
- Resource Organizations
- Resource Unit/Departments
- Resource Functions
- Resource Project/App/Services
- Custom Components
- Resource Delimiters

---

## Name Generation API

### RequestName (Recommended)

**Endpoint**: `POST /api/ResourceNamingRequests/RequestName`

**Description**: Generate a resource name using simplified request format. This is the **recommended** method for most scenarios.

**API Key**: Name Generation API Access Key or Full API Access Key

**Request Body**:
```json
{
  "resourceEnvironment": "prod",
  "resourceFunction": "data",
  "resourceInstance": "001",
  "resourceLocation": "eastus",
  "resourceOrg": "contoso",
  "resourceProjAppSvc": "webapp",
  "resourceType": "vnet",
  "resourceUnitDept": "marketing"
}
```

**Field Descriptions**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `resourceType` | string | ✅ Yes | The short name of the Azure resource type (e.g., "vnet", "st", "kv") |
| `resourceEnvironment` | string | Optional | Environment identifier (e.g., "prod", "dev", "test") |
| `resourceLocation` | string | Optional | Azure region short name (e.g., "eastus", "westeu") |
| `resourceOrg` | string | Optional | Organization identifier |
| `resourceUnitDept` | string | Optional | Business unit or department |
| `resourceFunction` | string | Optional | Resource function/purpose (e.g., "data", "app", "web") |
| `resourceProjAppSvc` | string | Optional | Project, application, or service name |
| `resourceInstance` | string | Optional | Instance number (e.g., "001", "002") |

**Success Response (200 OK)**:
```json
{
  "resourceName": "vnet-contoso-marketing-webapp-data-prod-eastus-001",
  "message": "Name generation successful!",
  "success": true
}
```

**Error Response (400 Bad Request)**:
```json
{
  "resourceName": "",
  "message": "Resource type 'invalid-type' not found",
  "success": false
}
```

**Example (cURL)**:
```bash
curl -X POST "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName" \
  -H "APIKey: your-name-gen-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "resourceType": "vnet",
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus",
    "resourceInstance": "001"
  }'
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

Invoke-RestMethod -Uri "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName" `
    -Method Post `
    -Headers $headers `
    -Body $body
```

**Example (Python)**:
```python
import requests
import json

url = "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName"
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
print(response.json())
```

---

### RequestNameWithComponents

**Endpoint**: `POST /api/ResourceNamingRequests/RequestNameWithComponents`

**Description**: Generate a resource name with full component object definitions. Use this when you need precise control over component IDs and values.

**API Key**: Name Generation API Access Key or Full API Access Key

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
    },
    {
      "id": 3,
      "name": "Resource Location",
      "value": "eastus"
    },
    {
      "id": 4,
      "name": "Resource Instance",
      "value": "001"
    }
  ]
}
```

**Success Response (200 OK)**:
```json
{
  "resourceName": "vnet-prod-eastus-001",
  "message": "Name generation successful!",
  "success": true
}
```

**Note**: This method requires knowledge of component IDs and exact naming. For most scenarios, use `RequestName` instead.

---

### ValidateName

**Endpoint**: `POST /api/ResourceNamingRequests/ValidateName`

**Description**: Validate a resource name against the Azure resource type regex pattern. This does NOT validate against the tool's configuration, only against Azure's naming rules.

**API Key**: Name Generation API Access Key or Full API Access Key

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
  "valid": true,
  "message": "Name is valid for resource type 'vnet'"
}
```

**Invalid Name Response (200 OK)**:
```json
{
  "valid": false,
  "message": "Name violates Azure naming rules: must be lowercase alphanumeric with hyphens"
}
```

**Example (cURL)**:
```bash
curl -X POST "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/ValidateName" \
  -H "APIKey: your-name-gen-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "resourceType": "vnet",
    "name": "vnet-contoso-prod-eastus-001"
  }'
```

---

## Configuration Endpoints

All configuration endpoints follow a consistent pattern:

### Resource Types

**List All**: `GET /api/ResourceTypes`

**Example Response**:
```json
[
  {
    "id": 1,
    "resource": "Virtual Network",
    "shortName": "vnet",
    "scope": "resource group",
    "lengthMin": 2,
    "lengthMax": 64,
    "validText": "Alphanumerics, underscores, periods, and hyphens.",
    "regx": "^[a-zA-Z0-9][a-zA-Z0-9-._]{0,62}[a-zA-Z0-9_]$",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Locations

**List All**: `GET /api/ResourceLocations`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "East US",
    "shortName": "eastus",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Environments

**List All**: `GET /api/ResourceEnvironments`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Production",
    "shortName": "prod",
    "enabled": true
  },
  {
    "id": 2,
    "name": "Development",
    "shortName": "dev",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Organizations

**List All**: `GET /api/ResourceOrgs`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Contoso Corporation",
    "shortName": "contoso",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Unit/Departments

**List All**: `GET /api/ResourceUnitDepts`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Marketing",
    "shortName": "marketing",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Functions

**List All**: `GET /api/ResourceFunctions`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Data",
    "shortName": "data",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Project/App/Services

**List All**: `GET /api/ResourceProjAppSvcs`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Web Application",
    "shortName": "webapp",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Custom Components

**List All**: `GET /api/CustomComponents`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Region Code",
    "shortName": "us",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

### Resource Delimiters

**List All**: `GET /api/ResourceDelimiters`

**Example Response**:
```json
[
  {
    "id": 1,
    "name": "Hyphen",
    "delimiter": "-",
    "enabled": true
  }
]
```

**API Key**: Any (Read-Only, Name Generation, or Full Access)

---

## Administrative Endpoints

### Admin Settings

**Update Password**: `POST /api/Admin/UpdatePassword`

**Headers**:
```http
AdminPassword: current-global-admin-password
Content-Type: application/json
```

**Request Body**:
```json
"new-password"
```

**Response**: `"SUCCESS"` or `"FAILURE - ..."`

---

**Update Full API Key**: `POST /api/Admin/UpdateAPIKey`

**Headers**:
```http
AdminPassword: current-global-admin-password
Content-Type: application/json
```

**Request Body**:
```json
"new-api-key"
```

**Response**: `"SUCCESS"` or `"FAILURE - ..."`

---

### Generated Names Log

**List Generated Names**: `GET /api/Admin/GetGeneratedNames`

**API Key**: Full API Access Key

**Example Response**:
```json
[
  {
    "id": 1,
    "resourceName": "vnet-contoso-prod-eastus-001",
    "resourceType": "vnet",
    "createdBy": "API",
    "createdOn": "2025-01-15T10:30:00Z"
  }
]
```

---

### Import/Export

**Export Configuration**: `GET /api/ImportExport/ExportConfig`

**API Key**: Full API Access Key

**Response**: JSON configuration file download

---

**Import Configuration**: `POST /api/ImportExport/ImportConfig`

**API Key**: Full API Access Key

**Request Body**: Multipart form data with configuration JSON file

---

## Response Formats

### Success Responses

Name generation endpoints return:
```json
{
  "resourceName": "vnet-contoso-prod-eastus-001",
  "message": "Name generation successful!",
  "success": true
}
```

Configuration endpoints (GET) return arrays of objects:
```json
[
  { "id": 1, "name": "...", "shortName": "...", "enabled": true }
]
```

### Error Responses

**401 Unauthorized** (Invalid/Missing API Key):
```
"Api Key was not provided!"
```
or
```
"Api Key is not valid!"
```

**400 Bad Request** (Invalid Request):
```json
{
  "resourceName": "",
  "message": "Resource type 'invalid-type' not found",
  "success": false
}
```

**500 Internal Server Error**:
```
"Error message describing the issue"
```

---

## Best Practices

### 1. Use the Correct API Key for Each Scenario

- **CI/CD Pipelines**: Use Name Generation API Access Key
- **Read-Only Tools**: Use Read-Only API Access Key
- **Full Administration**: Use Full API Access Key (securely)

### 2. Use RequestName for Most Scenarios

The `RequestName` endpoint is simpler and recommended for most use cases. Use `RequestNameWithComponents` only when you need precise component ID control.

### 3. Cache Configuration Data

Configuration data (resource types, locations, etc.) changes infrequently. Cache this data to reduce API calls.

### 4. Handle Errors Gracefully

Always check the `success` field in responses and handle errors appropriately:

```python
response = requests.post(url, headers=headers, json=data)
result = response.json()

if result.get("success"):
    name = result["resourceName"]
    print(f"Generated name: {name}")
else:
    error_message = result.get("message", "Unknown error")
    print(f"Error: {error_message}")
```

### 5. Store API Keys Securely

- Use environment variables or secret management systems (Azure Key Vault, AWS Secrets Manager)
- Never commit API keys to source control
- Rotate keys regularly

### 6. Use HTTPS

Always use HTTPS in production to protect API keys during transmission.

### 7. Validate Inputs Before Calling the API

Validate resource types and component values against your configuration before making API calls to reduce errors.

---

## Error Handling

### Common Error Scenarios

| Error Code | Scenario | Solution |
|------------|----------|----------|
| 401 Unauthorized | Missing or invalid API key | Verify API key is correct and included in headers |
| 400 Bad Request | Invalid resource type | Check resource type against configured types |
| 400 Bad Request | Missing required fields | Ensure `resourceType` is provided |
| 500 Internal Server Error | Server error | Check server logs, contact administrator |

### Retry Logic

Implement exponential backoff for transient errors:

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
                time.sleep(2 ** attempt)  # Exponential backoff
            else:
                # Permanent error - don't retry
                return response.json()
        except requests.exceptions.RequestException as e:
            time.sleep(2 ** attempt)
    
    raise Exception("Max retries exceeded")
```

---

## Rate Limiting

The V1 API does not currently enforce rate limiting. However, best practices recommend:

- Limit concurrent requests to 10-20 per second
- Cache configuration data
- Use batch operations when available
- Implement client-side throttling

---

## Migration to V2

Consider migrating to [API V2](API-V2-WIKI) for:

✅ **Standardized Response Format**: Consistent `ApiResponse<T>` wrapper  
✅ **Enhanced Error Handling**: Detailed error codes and messages  
✅ **Azure Tenant Validation**: Real-time name conflict detection  
✅ **Correlation IDs**: Better request tracking and debugging  
✅ **Improved HTTP Status Codes**: Proper use of 400, 401, 500 codes  
✅ **Metadata**: Timestamps, versioning, and pagination support  

Migration is straightforward - V2 endpoints have the same functionality with enhanced response formats.

---

## Examples

### Example 1: Generate Virtual Network Name (PowerShell)

```powershell
# Configuration
$apiUrl = "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName"
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
    Write-Host "Generated name: $($response.resourceName)"
} else {
    Write-Error "Error: $($response.message)"
}
```

---

### Example 2: Get All Resource Types (Python)

```python
import requests

url = "https://your-naming-tool.azurewebsites.net/api/ResourceTypes"
headers = {
    "APIKey": "your-readonly-api-key"
}

response = requests.get(url, headers=headers)
resource_types = response.json()

# Print enabled resource types
for rt in resource_types:
    if rt["enabled"]:
        print(f"{rt['resource']} ({rt['shortName']})")
```

---

### Example 3: Terraform Integration

```hcl
# Call Azure Naming Tool API from Terraform
data "http" "resource_name" {
  url    = "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName"
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
  vnet_name = jsondecode(data.http.resource_name.response_body).resourceName
}

resource "azurerm_virtual_network" "example" {
  name                = local.vnet_name
  location            = "eastus"
  resource_group_name = azurerm_resource_group.example.name
  address_space       = ["10.0.0.0/16"]
}
```

---

### Example 4: Azure DevOps Pipeline Integration

```yaml
# azure-pipelines.yml
trigger:
  - main

variables:
  namingToolUrl: 'https://your-naming-tool.azurewebsites.net/api'
  namingToolApiKey: $(NamingToolApiKey)  # Store in Azure DevOps secure variables

steps:
  - task: PowerShell@2
    displayName: 'Generate Resource Name'
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

        $response = Invoke-RestMethod -Uri "$(namingToolUrl)/ResourceNamingRequests/RequestName" `
            -Method Post `
            -Headers $headers `
            -Body $body

        Write-Host "##vso[task.setvariable variable=StorageAccountName]$($response.resourceName)"

  - task: AzureCLI@2
    displayName: 'Create Storage Account'
    inputs:
      azureSubscription: 'Azure-Subscription'
      scriptType: 'bash'
      scriptLocation: 'inlineScript'
      inlineScript: |
        az storage account create \
          --name $(StorageAccountName) \
          --resource-group my-rg \
          --location $(Location) \
          --sku Standard_LRS
```

---

## Additional Resources

- **Swagger UI**: `https://your-naming-tool-url/swagger` (interactive API documentation)
- **API V2 Documentation**: [Using the API V2](API-V2-WIKI) (enhanced features)
- **Azure Validation Guide**: [Azure Validation WIKI](AZURE-VALIDATION-WIKI) (V2 feature)
- **GitHub Repository**: [Azure Naming Tool](https://github.com/mspnp/AzureNamingTool)

---

## Support and Feedback

- **Issues**: [GitHub Issues](https://github.com/mspnp/AzureNamingTool/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mspnp/AzureNamingTool/discussions)
- **Documentation**: [GitHub Wiki](https://github.com/mspnp/AzureNamingTool/wiki)
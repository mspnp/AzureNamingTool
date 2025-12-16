# Azure Naming Tool API Documentation

Welcome to the Azure Naming Tool API documentation. The API provides programmatic access to generate standardized Azure resource names based on your organization's naming conventions.

---

## Available API Versions

### API Version 2.0 (Recommended)

**[View Full V2 Documentation →](API-V2)**

The latest API version with enhanced features, standardized responses, and Azure tenant validation.

**Key Features:**
- ✅ Standardized `ApiResponse<T>` wrapper for all responses
- ✅ Azure tenant validation (real-time conflict detection)
- ✅ Automatic conflict resolution strategies
- ✅ Correlation IDs for request tracking
- ✅ Enhanced error handling with detailed error codes
- ✅ Proper HTTP status code usage (400, 401, 500)
- ✅ Response metadata (timestamps, versioning)

**Endpoint Pattern:**
```
POST /api/v2.0/ResourceNamingRequests/RequestName
GET /api/v2.0/ResourceTypes
```

**When to Use:**
- New integrations and projects
- When you need Azure tenant validation
- When you need detailed error handling and tracking
- When you want standardized response formats

---

### API Version 1.0 (Stable)

**[View Full V1 Documentation →](API-V1)**

The original, production-ready API that remains fully supported for backward compatibility.

**Key Features:**
- ✅ Stable and battle-tested
- ✅ Simple request/response format
- ✅ Generate names using naming conventions
- ✅ Validate names against Azure regex patterns
- ✅ Manage configuration components
- ✅ Full backward compatibility

**Endpoint Pattern:**
```
POST /api/ResourceNamingRequests/RequestName
GET /api/ResourceTypes
```

**When to Use:**
- Existing integrations using V1
- When you prefer simple response formats
- When Azure tenant validation is not required
- Legacy applications requiring stability

---

## Quick Start

### 1. Get Your API Key

Navigate to **Admin → API Keys** in the Azure Naming Tool to generate an API key.

**Three Key Types:**
- 🔑 **Full API Access** - Complete administrative access
- 🔧 **Name Generation** - Generate names and read configurations
- 👁️ **Read-Only** - View-only access to configurations

### 2. Choose Your API Version

- **V2** (Recommended) - For new projects requiring advanced features
- **V1** - For existing integrations or simple use cases

### 3. Make Your First Request

**V2 Example:**
```bash
curl -X POST "https://your-naming-tool.azurewebsites.net/api/v2.0/ResourceNamingRequests/RequestName" \
  -H "APIKey: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "resourceType": "vnet",
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus"
  }'
```

**V1 Example:**
```bash
curl -X POST "https://your-naming-tool.azurewebsites.net/api/ResourceNamingRequests/RequestName" \
  -H "APIKey: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "resourceType": "vnet",
    "resourceEnvironment": "prod",
    "resourceLocation": "eastus"
  }'
```

---

## Interactive Documentation (Swagger UI)

Access the interactive API documentation at:
```
https://your-naming-tool-url/swagger
```

**Features:**
- Explore all V1 and V2 endpoints
- Test API calls directly from your browser
- View request/response schemas
- Authenticate with your API key

---

## Common Use Cases

### CI/CD Pipelines
Generate resource names during automated deployments using Azure DevOps, GitHub Actions, or Jenkins.

**Recommended:** API V2 with Name Generation key

---

### Infrastructure as Code (IaC)
Integrate with Terraform, Bicep, or ARM templates to generate compliant resource names.

**Recommended:** API V2 with Name Generation key

---

### Automated Provisioning
Build custom provisioning scripts that generate standardized names for your resources.

**Recommended:** API V2 with Name Generation key

---

### Configuration Management
Automate updates to naming convention components and configurations.

**Recommended:** API V2 with Full API Access key

---

### Auditing and Reporting
Query configurations and naming history for compliance reporting.

**Recommended:** API V1 or V2 with Read-Only key

---

## Key Differences Between V1 and V2

| Feature | V1 | V2 |
|---------|----|----|
| **Response Format** | Direct JSON response | Standardized `ApiResponse<T>` wrapper |
| **HTTP Status Codes** | Mostly 200 OK | Proper codes (200, 400, 401, 500) |
| **Azure Validation** | ❌ Not available | ✅ Real-time Azure conflict detection |
| **Correlation IDs** | ❌ Not available | ✅ Included in all responses |
| **Error Details** | Basic error messages | Detailed error codes and nested errors |
| **URL Pattern** | `/api/Controller/Action` | `/api/v2.0/Controller/Action` |
| **Backward Compatible** | Original API | Maintains V1 functionality |

---

## Authentication

All API requests require authentication using an API key in the request header:

```http
APIKey: your-api-key-here
```

API keys are managed in **Admin → API Keys** section of the Azure Naming Tool.

**Security Best Practices:**
- Store API keys securely (Azure Key Vault, environment variables)
- Never commit keys to source control
- Use different keys for dev/staging/production
- Rotate keys regularly
- Use appropriate key types (Read-Only for auditing, Name Generation for CI/CD)

---

## Additional Resources

### Azure Tenant Validation
Learn how to enable real-time Azure resource name validation in API V2:

**[Azure Validation Documentation →](AZURE-NAME-VALIDATION-GUIDE)**

---

### Migration Guide
Upgrading to v5.0.0 or migrating from V1 to V2?

**[v5.0.0 Migration Guide →](V5.0.0-MIGRATION-GUIDE)**

---

### Release Notes
See what's new in the latest version:

**[v5.0.0 Release Notes →](v5.0.0)**

---

## Support and Feedback

- **Issues**: [GitHub Issues](https://github.com/mspnp/AzureNamingTool/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mspnp/AzureNamingTool/discussions)
- **Documentation**: [GitHub Wiki](https://github.com/mspnp/AzureNamingTool/wiki)
- **Repository**: [Azure Naming Tool](https://github.com/mspnp/AzureNamingTool)

---

**Ready to get started?**
- [API V2 Documentation →](API-V2)
- [API V1 Documentation →](API-V1)
- [Azure Validation Setup →](AZURE-NAME-VALIDATION-GUIDE)

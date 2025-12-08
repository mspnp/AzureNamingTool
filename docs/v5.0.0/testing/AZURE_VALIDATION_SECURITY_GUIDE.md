# Azure Validation Security Guide

## Overview
This guide covers security best practices, credential management, and RBAC requirements for the Azure tenant name validation feature.

---

## Table of Contents
1. [Authentication Methods](#authentication-methods)
2. [RBAC Requirements](#rbac-requirements)
3. [Credential Storage](#credential-storage)
4. [Key Vault Integration](#key-vault-integration)
5. [Security Best Practices](#security-best-practices)
6. [Troubleshooting](#troubleshooting)

---

## Authentication Methods

The Azure Naming Tool supports two authentication methods for Azure validation:

### 1. Managed Identity (Recommended)

**Use When:**
- Hosting in Azure App Service
- Hosting in Azure Container Apps
- Hosting in Azure Kubernetes Service (AKS)
- Hosting in any Azure service that supports Managed Identity

**Benefits:**
- ✅ No credentials to manage or rotate
- ✅ Automatic authentication with Azure
- ✅ No secrets stored in configuration files
- ✅ Follows Azure security best practices
- ✅ Zero-trust security model

**Setup:**
1. Enable System-assigned Managed Identity on your Azure service
2. Assign **Reader** role to the Managed Identity on target subscriptions
3. Configure Azure Naming Tool to use Managed Identity authentication
4. No additional credentials required!

**Configuration:**
```json
{
  "Enabled": true,
  "AuthMode": "ManagedIdentity",
  "SubscriptionIds": ["sub-id-1", "sub-id-2"],
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 5
  }
}
```

---

### 2. Service Principal

**Use When:**
- Hosting on-premises
- Hosting in non-Azure cloud providers
- Testing locally during development
- Managed Identity is not available

**Security Considerations:**
- ⚠️ Requires managing client secrets
- ⚠️ Secrets must be stored securely
- ⚠️ Secrets must be rotated periodically (recommended: 90 days)
- ⚠️ Consider using Azure Key Vault for secret storage

**Setup:**
1. Create an Azure AD App Registration
2. Generate a client secret
3. Assign **Reader** role on target subscriptions
4. Store credentials securely (see [Credential Storage](#credential-storage))

---

## RBAC Requirements

### Minimum Required Permissions

The Azure Naming Tool requires **Reader** role on subscriptions to validate resource names.

#### What "Reader" Role Provides:
- ✅ List and query resources via Resource Graph
- ✅ Call CheckNameAvailability API for global resources
- ✅ View subscription details
- ❌ **Cannot** create, modify, or delete any resources
- ❌ **Cannot** access sensitive data (keys, connection strings, etc.)

### Role Assignment

**For Managed Identity:**
```bash
# Get the Managed Identity principal ID
$principalId = az webapp identity show --name <app-name> --resource-group <rg-name> --query principalId -o tsv

# Assign Reader role on subscription
az role assignment create \
  --assignee $principalId \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

**For Service Principal:**
```bash
# Assign Reader role to Service Principal
az role assignment create \
  --assignee <client-id> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

### Multi-Subscription Access

To validate names across multiple subscriptions, assign the Reader role on each:

```bash
# Loop through subscriptions
$subscriptions = @("sub-id-1", "sub-id-2", "sub-id-3")

foreach ($sub in $subscriptions) {
    az role assignment create \
      --assignee $principalId \
      --role "Reader" \
      --scope "/subscriptions/$sub"
}
```

### Management Group Scope (Optional)

For organizations with many subscriptions, assign at management group level:

```bash
az role assignment create \
  --assignee $principalId \
  --role "Reader" \
  --scope "/providers/Microsoft.Management/managementGroups/<mg-id>"
```

---

## Credential Storage

### Option 1: Azure Key Vault (Highly Recommended)

**Benefits:**
- 🔒 Secrets encrypted at rest
- 🔒 Access logged and audited
- 🔒 Automatic secret rotation support
- 🔒 Managed Identity can retrieve secrets
- 🔒 No secrets in configuration files

**Setup Steps:**

1. **Create Azure Key Vault:**
```bash
az keyvault create \
  --name <keyvault-name> \
  --resource-group <rg-name> \
  --location <region>
```

2. **Store Service Principal Secret:**
```bash
az keyvault secret set \
  --vault-name <keyvault-name> \
  --name "naming-tool-client-secret" \
  --value "<client-secret-value>"
```

3. **Grant Access to Managed Identity:**
```bash
# Get the Managed Identity principal ID
$principalId = az webapp identity show --name <app-name> --resource-group <rg-name> --query principalId -o tsv

# Grant Key Vault Secrets User role
az role assignment create \
  --assignee $principalId \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/<sub-id>/resourceGroups/<rg-name>/providers/Microsoft.KeyVault/vaults/<kv-name>"
```

4. **Configure Azure Naming Tool:**
```json
{
  "Enabled": true,
  "AuthMode": "ServicePrincipal",
  "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "ServicePrincipal": {
    "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientSecretKeyVaultName": "naming-tool-client-secret"
  },
  "KeyVault": {
    "KeyVaultUri": "https://<keyvault-name>.vault.azure.net/",
    "ClientSecretName": "naming-tool-client-secret"
  }
}
```

**How It Works:**
1. Azure Naming Tool uses its Managed Identity to authenticate to Key Vault
2. Retrieves the Service Principal client secret from Key Vault
3. Uses the secret to authenticate to Azure Resource Manager
4. Validates names against Azure tenant

---

### Option 2: Configuration File (Development Only)

**⚠️ WARNING: Only use for development/testing. NOT for production!**

**Configuration:**
```json
{
  "Enabled": true,
  "AuthMode": "ServicePrincipal",
  "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "ServicePrincipal": {
    "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientSecret": "your-client-secret-here"
  }
}
```

**Security Risks:**
- ❌ Secret stored in plain text
- ❌ Secret visible in file system
- ❌ Secret may be committed to source control
- ❌ No audit trail for secret access
- ❌ No automatic rotation

**Mitigation (if you must use this option):**
1. Ensure `settings/azurevalidationsettings.json` is in `.gitignore`
2. Use file system permissions to restrict access
3. Rotate secrets frequently (every 30 days)
4. Monitor for unauthorized access
5. Plan migration to Key Vault

---

## Key Vault Integration

### Architecture Diagram

```
┌─────────────────────┐
│ Azure Naming Tool   │
│ (Managed Identity)  │
└──────────┬──────────┘
           │ 1. Authenticate with MI
           ▼
┌─────────────────────┐
│  Azure Key Vault    │
│  ┌───────────────┐  │
│  │ Client Secret │  │ 2. Retrieve Secret
│  └───────────────┘  │
└──────────┬──────────┘
           │ 3. Return Secret
           ▼
┌─────────────────────┐
│ Service Principal   │
│ Authentication      │
└──────────┬──────────┘
           │ 4. Authenticate to ARM
           ▼
┌─────────────────────┐
│ Azure Resource      │
│ Manager (ARM)       │
│ - Resource Graph    │
│ - CheckNameAvail    │
└─────────────────────┘
```

### Implementation Details

**AzureValidationService Flow:**
```csharp
1. Check AuthMode
2. IF ManagedIdentity:
     → Use DefaultAzureCredential (automatic)
3. IF ServicePrincipal:
     → Check if KeyVault configured
     → IF KeyVault:
        a. Use Managed Identity to auth to Key Vault
        b. Retrieve client secret from Key Vault
        c. Create ClientSecretCredential
     → ELSE:
        a. Read client secret from configuration file
        b. Create ClientSecretCredential
4. Use credential to call ARM APIs
```

### Key Vault Best Practices

1. **Secret Naming Convention:**
   - Use descriptive names: `naming-tool-client-secret`
   - Include environment: `naming-tool-client-secret-prod`
   - Version secrets: `naming-tool-client-secret-v2`

2. **Access Policies:**
   - Use RBAC (not legacy access policies)
   - Grant minimum required permissions (Secrets User)
   - Audit access logs regularly

3. **Secret Rotation:**
   - Enable automatic rotation if possible
   - Set expiration dates on secrets
   - Monitor for expiring secrets
   - Update configuration when rotating

4. **Networking:**
   - Enable Key Vault firewall
   - Allow only Azure Naming Tool subnet
   - Use private endpoints for production

---

## Security Best Practices

### 1. Principle of Least Privilege
- ✅ Grant only **Reader** role (never Contributor or Owner)
- ✅ Scope to specific subscriptions (not Management Group unless necessary)
- ✅ Use Managed Identity whenever possible
- ✅ Rotate Service Principal secrets every 90 days maximum

### 2. Credential Hygiene
- ✅ Never commit secrets to source control
- ✅ Use Key Vault for production deployments
- ✅ Set expiration dates on all secrets
- ✅ Monitor for expiring/expired credentials
- ✅ Rotate immediately if compromise suspected

### 3. Monitoring & Auditing
- ✅ Enable diagnostic logging on Azure Naming Tool
- ✅ Monitor admin log for validation activity
- ✅ Review Key Vault access logs monthly
- ✅ Set alerts for authentication failures
- ✅ Track Resource Graph query patterns

### 4. Network Security
- ✅ Use HTTPS only (enforce in App Service)
- ✅ Enable App Service Authentication if possible
- ✅ Restrict admin access to known IP addresses
- ✅ Use private endpoints for Key Vault access
- ✅ Enable Azure DDoS protection

### 5. Configuration Security
- ✅ Store `azurevalidationsettings.json` outside web root if possible
- ✅ Encrypt sensitive configuration at rest
- ✅ Use environment variables for non-secret config
- ✅ Validate all user input in admin UI
- ✅ Log all configuration changes

### 6. Operational Security
- ✅ Review RBAC assignments quarterly
- ✅ Remove unused Service Principals
- ✅ Test disaster recovery procedures
- ✅ Document runbooks for incident response
- ✅ Keep Azure SDK packages up to date

---

## Troubleshooting

### Common Issues

#### 1. "Authentication Failed" Error

**Possible Causes:**
- Managed Identity not enabled
- Service Principal credentials incorrect
- Client secret expired
- Key Vault permissions missing

**Resolution:**
```bash
# Check Managed Identity status
az webapp identity show --name <app-name> --resource-group <rg-name>

# Verify Service Principal exists
az ad sp show --id <client-id>

# Test Key Vault access
az keyvault secret show --vault-name <kv-name> --name <secret-name>
```

#### 2. "Insufficient Permissions" Error

**Possible Causes:**
- Reader role not assigned
- Role assignment on wrong scope
- Role assignment not propagated (can take 5-10 minutes)

**Resolution:**
```bash
# List role assignments
az role assignment list --assignee <principal-id> --scope "/subscriptions/<sub-id>"

# Verify Reader role exists
az role assignment list --assignee <principal-id> --query "[?roleDefinitionName=='Reader']"
```

#### 3. "Resource Graph Query Failed" Error

**Possible Causes:**
- Subscription not registered for Resource Graph
- Query timeout (5 seconds default)
- Invalid KQL syntax

**Resolution:**
```bash
# Register Resource Graph provider
az provider register --namespace Microsoft.ResourceGraph

# Check registration status
az provider show --namespace Microsoft.ResourceGraph --query registrationState
```

#### 4. "Key Vault Access Denied" Error

**Possible Causes:**
- Managed Identity doesn't have Key Vault access
- Key Vault firewall blocking requests
- Secret name incorrect

**Resolution:**
```bash
# Grant Key Vault access
az role assignment create \
  --assignee <managed-identity-principal-id> \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.KeyVault/vaults/<kv-name>"

# Check firewall rules
az keyvault network-rule list --name <kv-name>
```

#### 5. "CheckNameAvailability API Failed" Error

**Possible Causes:**
- Resource provider not registered
- API version incorrect
- Rate limiting / throttling

**Resolution:**
```bash
# Register all required providers
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.KeyVault
# ... etc for each resource type

# Check registration status
az provider list --query "[?registrationState=='Registered'].namespace" -o tsv
```

---

## Security Checklist

Before deploying to production:

- [ ] Managed Identity enabled (if hosting in Azure)
- [ ] Reader role assigned on all target subscriptions
- [ ] Service Principal secret stored in Key Vault (if using SP)
- [ ] Key Vault access granted to Managed Identity
- [ ] No secrets in configuration files
- [ ] `azurevalidationsettings.json` in `.gitignore`
- [ ] HTTPS enforced on App Service
- [ ] App Service Authentication enabled
- [ ] Diagnostic logging enabled
- [ ] Admin log monitoring configured
- [ ] Secret expiration dates set
- [ ] Secret rotation procedure documented
- [ ] Incident response plan created
- [ ] Quarterly RBAC review scheduled
- [ ] Azure SDK packages up to date

---

## Additional Resources

- [Azure Managed Identity Documentation](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [Azure RBAC Documentation](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Azure Resource Graph Documentation](https://learn.microsoft.com/en-us/azure/governance/resource-graph/)
- [CheckNameAvailability API Reference](https://learn.microsoft.com/en-us/rest/api/resources/check-name-availability)

---

*Document Version: 1.0*  
*Last Updated: January 2025*  
*Applies to: Azure Naming Tool v5.0.0+*

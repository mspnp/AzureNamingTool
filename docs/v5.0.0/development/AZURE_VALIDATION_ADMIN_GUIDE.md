# Azure Validation Administrator Guide

## Overview
This comprehensive guide walks administrators through setting up and configuring Azure tenant name validation for the Azure Naming Tool.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Setup Scenarios](#setup-scenarios)
4. [Configuration](#configuration)
5. [Testing & Verification](#testing--verification)
6. [Troubleshooting](#troubleshooting)
7. [Maintenance](#maintenance)

---

## Prerequisites

### Azure Requirements
- ✅ Azure subscription(s) to validate names against
- ✅ Azure AD tenant access
- ✅ Permissions to create Service Principals (if not using Managed Identity)
- ✅ Permissions to assign RBAC roles on subscriptions

### Azure Naming Tool Requirements
- ✅ Azure Naming Tool v5.0.0 or later
- ✅ Global Admin password access
- ✅ Azure SDK packages installed (included by default)

### Hosting Requirements
- **For Managed Identity**: Azure App Service, Container Apps, or AKS
- **For Service Principal**: Any hosting platform (Azure or on-premises)

---

## Quick Start

### 🚀 5-Minute Setup (Managed Identity - Recommended)

**Prerequisites:** Azure Naming Tool deployed to Azure App Service

```bash
# Step 1: Enable Managed Identity on App Service
az webapp identity assign \
  --name <your-app-name> \
  --resource-group <your-rg>

# Step 2: Get the Managed Identity Principal ID
$principalId = az webapp identity show \
  --name <your-app-name> \
  --resource-group <your-rg> \
  --query principalId -o tsv

# Step 3: Assign Reader role on subscription
az role assignment create \
  --assignee $principalId \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"

# Step 4: Enable Azure Validation in the tool
# Navigate to Admin > Site Settings > Enable "Azure Tenant Name Validation"

# Step 5: Configure Authentication
# Navigate to Admin > Azure Validation > Select "Managed Identity"

# Step 6: Test Connection
# Click "Test Connection" button to verify setup

# Done! ✅
```

---

## Setup Scenarios

### Scenario 1: Azure App Service with Managed Identity

**Best For:** Production deployments in Azure

**Steps:**

1. **Enable System-Assigned Managed Identity**

Via Azure Portal:
- Open your App Service
- Navigate to **Identity** → **System assigned**
- Toggle **Status** to **On**
- Click **Save**
- Copy the **Object (principal) ID**

Via Azure CLI:
```bash
az webapp identity assign \
  --name naming-tool-app \
  --resource-group naming-tool-rg
```

2. **Assign Reader Role**

For Single Subscription:
```bash
az role assignment create \
  --assignee <principal-id> \
  --role "Reader" \
  --scope "/subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

For Multiple Subscriptions:
```bash
# Create a script
$subscriptions = @(
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz"
)

foreach ($sub in $subscriptions) {
    Write-Host "Assigning Reader role on subscription $sub..."
    az role assignment create `
      --assignee <principal-id> `
      --role "Reader" `
      --scope "/subscriptions/$sub"
}
```

For Entire Management Group:
```bash
az role assignment create \
  --assignee <principal-id> \
  --role "Reader" \
  --scope "/providers/Microsoft.Management/managementGroups/<mg-id>"
```

3. **Configure Azure Naming Tool**

Navigate to **Admin** → **Site Settings**:
- Enable **Azure Tenant Name Validation** toggle

Navigate to **Admin** → **Azure Validation**:
- Select **Managed Identity** authentication mode
- Click **Save Settings**
- Click **Test Connection**

**Expected Result:**
```
✅ Connected to Azure
Authentication: ManagedIdentity
Tenant: <your-tenant-id>
Accessible Subscriptions: 3
```

---

### Scenario 2: Service Principal with Key Vault (Production)

**Best For:** On-premises deployments or non-Azure hosting with maximum security

**Steps:**

1. **Create Service Principal**

```bash
# Create the Service Principal
$sp = az ad sp create-for-rbac `
  --name "naming-tool-service-principal" `
  --skip-assignment `
  --output json | ConvertFrom-Json

# Save these values securely:
Write-Host "Tenant ID: $($sp.tenant)"
Write-Host "Client ID: $($sp.appId)"
Write-Host "Client Secret: $($sp.password)"  # ⚠️ Save this - you won't see it again!
```

2. **Assign Reader Role**

```bash
az role assignment create \
  --assignee $sp.appId \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

3. **Create Azure Key Vault**

```bash
# Create Key Vault
az keyvault create \
  --name naming-tool-kv \
  --resource-group naming-tool-rg \
  --location eastus

# Store the client secret in Key Vault
az keyvault secret set \
  --vault-name naming-tool-kv \
  --name "naming-tool-client-secret" \
  --value "<client-secret-from-step-1>"
```

4. **Grant App Service Access to Key Vault**

```bash
# Get the Managed Identity of the App Service
$appPrincipalId = az webapp identity show `
  --name naming-tool-app `
  --resource-group naming-tool-rg `
  --query principalId -o tsv

# Grant Key Vault access
az role assignment create \
  --assignee $appPrincipalId \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/<sub-id>/resourceGroups/naming-tool-rg/providers/Microsoft.KeyVault/vaults/naming-tool-kv"
```

5. **Configure Azure Naming Tool**

Edit `settings/azurevalidationsettings.json`:
```json
{
  "Enabled": true,
  "AuthMode": "ServicePrincipal",
  "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "SubscriptionIds": [
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  ],
  "ServicePrincipal": {
    "ClientId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "ClientSecretKeyVaultName": "naming-tool-client-secret"
  },
  "KeyVault": {
    "KeyVaultUri": "https://naming-tool-kv.vault.azure.net/",
    "ClientSecretName": "naming-tool-client-secret"
  },
  "ConflictResolution": {
    "Strategy": "AutoIncrement",
    "MaxAttempts": 100
  },
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 5
  }
}
```

OR use the Admin UI:
- Navigate to **Admin** → **Azure Validation**
- Select **Service Principal** authentication mode
- Enter **Tenant ID**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Enter **Client ID**: `yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy`
- Click **Save Settings**

6. **Test Connection**

Click **Test Connection** in the Admin UI

**Expected Result:**
```
✅ Connected to Azure
Authentication: ServicePrincipal
Tenant: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Accessible Subscriptions: 1
```

---

### Scenario 3: Service Principal with File Configuration (Development Only)

**Best For:** Local development and testing ONLY

**⚠️ WARNING: NOT recommended for production - secrets stored in plain text**

**Steps:**

1. **Create Service Principal** (same as Scenario 2, Step 1)

2. **Assign Reader Role** (same as Scenario 2, Step 2)

3. **Configure via File**

Edit `settings/azurevalidationsettings.json`:
```json
{
  "Enabled": true,
  "AuthMode": "ServicePrincipal",
  "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "SubscriptionIds": [
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  ],
  "ServicePrincipal": {
    "ClientId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "ClientSecret": "your-actual-secret-here"
  },
  "ConflictResolution": {
    "Strategy": "AutoIncrement"
  },
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 5
  }
}
```

4. **Secure the Configuration File**

```bash
# Add to .gitignore
echo "src/settings/azurevalidationsettings.json" >> .gitignore

# Set restrictive file permissions (Linux/Mac)
chmod 600 src/settings/azurevalidationsettings.json

# Windows: Right-click → Properties → Security → Advanced
# Remove all users except yourself and SYSTEM
```

5. **Test Connection** (same as other scenarios)

---

## Configuration

### Full Configuration Options

**File:** `src/settings/azurevalidationsettings.json`

```json
{
  // Enable/disable Azure validation feature
  "Enabled": true,

  // Authentication mode: "ManagedIdentity" or "ServicePrincipal"
  "AuthMode": "ManagedIdentity",

  // Azure AD Tenant ID
  "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",

  // List of subscription IDs to query for name validation
  "SubscriptionIds": [
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"
  ],

  // Optional: Management Group ID for querying across all child subscriptions
  "ManagementGroupId": "mg-production",

  // Service Principal settings (only if AuthMode = ServicePrincipal)
  "ServicePrincipal": {
    "ClientId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    
    // Option 1: Direct secret (NOT RECOMMENDED for production)
    "ClientSecret": "your-secret-here",
    
    // Option 2: Key Vault secret name (RECOMMENDED)
    "ClientSecretKeyVaultName": "naming-tool-client-secret"
  },

  // Key Vault settings (if using Key Vault for secrets)
  "KeyVault": {
    "KeyVaultUri": "https://your-keyvault.vault.azure.net/",
    "ClientSecretName": "naming-tool-client-secret"
  },

  // Conflict resolution settings
  "ConflictResolution": {
    // Strategy: "AutoIncrement", "NotifyOnly", "Fail", "SuffixRandom"
    "Strategy": "AutoIncrement",
    
    // Maximum auto-increment attempts before failing
    "MaxAttempts": 100,
    
    // Padding for instance numbers (3 = 001, 002, etc.)
    "IncrementPadding": 3,
    
    // Include warnings in responses
    "IncludeWarnings": true
  },

  // Cache settings for validation results
  "Cache": {
    // Enable caching to reduce Azure API calls
    "Enabled": true,
    
    // Cache duration in minutes
    "DurationMinutes": 5
  }
}
```

### Configuration via Admin UI

All settings can be configured through the web interface:

1. **Enable Feature**
   - Navigate to **Admin** → **Site Settings**
   - Toggle **Azure Tenant Name Validation** to ON
   - Click **Save**

2. **Configure Authentication**
   - Navigate to **Admin** → **Azure Validation**
   - Select authentication mode (Managed Identity or Service Principal)
   - If Service Principal:
     * Enter Tenant ID
     * Enter Client ID
     * (Secret should be in Key Vault or config file)
   - Click **Save Settings**

3. **Configure Conflict Resolution**
   - Select desired strategy:
     * **AutoIncrement** - Automatically increment instance number
     * **NotifyOnly** - Keep original name, show warning
     * **Fail** - Reject name generation with error
     * **SuffixRandom** - Add random suffix
   - Click **Save Strategy**

4. **Configure Cache**
   - Toggle **Enable Validation Cache** ON/OFF
   - Set **Cache Duration** (1-60 minutes)
   - Click **Save Cache Settings**

5. **Test Connection**
   - Click **Test Connection** button
   - Verify successful connection and subscription count

---

## Testing & Verification

### Pre-Deployment Testing

**1. Test Authentication**

```bash
# Verify Managed Identity has access
az login --identity  # Run from App Service

# Test Resource Graph query
az graph query -q "Resources | project name, type | limit 5"

# Expected: List of 5 resources
```

**2. Test API Access**

Using the Azure Naming Tool API:

```powershell
# Generate a name
$headers = @{
    "APIKey" = "your-api-key"
    "Content-Type" = "application/json"
}

$body = @{
    resourceType = "st"
    resourceEnvironment = "prod"
    resourceLocation = "eastus"
    resourceInstance = "001"
} | ConvertTo-Json

$result = Invoke-RestMethod `
    -Uri "https://your-app.azurewebsites.net/api/v2/ResourceNamingRequests/RequestName" `
    -Method Post `
    -Headers $headers `
    -Body $body

# Check validation metadata
$result.validationMetadata
```

**Expected Response:**
```json
{
  "resourceName": "stprodeastus001",
  "success": true,
  "validationMetadata": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationTimestamp": "2025-01-15T10:30:00Z"
  }
}
```

### Post-Deployment Validation

**Test Scenarios:**

**✅ Test 1: Available Name**
- Generate a name that doesn't exist
- Expected: `validationMetadata.existsInAzure = false`
- Expected: Green badge "Available" in UI

**✅ Test 2: Existing Name**
- Create a storage account manually: `sttest001`
- Generate name `sttest001`
- Expected: `validationMetadata.existsInAzure = true`
- Expected: AutoIncrement to `sttest002` (or other strategy)

**✅ Test 3: Global Resource (Storage)**
- Generate storage account name
- Expected: Uses CheckNameAvailability API
- Expected: Validates against global namespace

**✅ Test 4: Scoped Resource (VNet)**
- Generate virtual network name
- Expected: Uses Resource Graph query
- Expected: Validates within tenant subscriptions

**✅ Test 5: Conflict Resolution**
- Create resources: `vnet-001`, `vnet-002`
- Generate `vnet-001`
- Expected AutoIncrement: `vnet-003`
- Expected NotifyOnly: `vnet-001` with warning
- Expected Fail: Error message
- Expected SuffixRandom: `vnet-001-abc123`

**✅ Test 6: Cache Behavior**
- Generate name twice quickly
- Expected: Second call faster (cached result)
- Wait cache duration + 1 minute
- Generate again
- Expected: Fresh validation

**✅ Test 7: Multi-Subscription**
- Configure multiple subscription IDs
- Create resource in subscription B
- Generate name in subscription A
- Expected: Still detects conflict (queries all subscriptions)

---

## Troubleshooting

### Issue: "Authentication Failed"

**Symptoms:**
- Test Connection shows ❌ error
- Generated names show "Not Validated" badge

**Diagnosis:**
```bash
# Check Managed Identity exists
az webapp identity show --name <app-name> --resource-group <rg-name>

# Check Service Principal exists (if using SP)
az ad sp show --id <client-id>

# Verify Key Vault access (if using KV)
az keyvault secret show --vault-name <kv-name> --name <secret-name>
```

**Resolution:**
1. Verify Managed Identity is enabled (System-assigned)
2. Verify Service Principal credentials are correct
3. Check client secret hasn't expired
4. Test Key Vault access manually
5. Review admin logs for detailed error messages

---

### Issue: "Insufficient Permissions"

**Symptoms:**
- Test Connection succeeds but validation fails
- Error: "Authorization failed" or "Forbidden"

**Diagnosis:**
```bash
# List role assignments
az role assignment list \
  --assignee <principal-id> \
  --scope "/subscriptions/<sub-id>"

# Check for Reader role specifically
az role assignment list \
  --assignee <principal-id> \
  --query "[?roleDefinitionName=='Reader']"
```

**Resolution:**
1. Assign Reader role: `az role assignment create --assignee <id> --role "Reader" --scope "/subscriptions/<sub>"`
2. Wait 5-10 minutes for propagation
3. Test connection again
4. Verify subscription ID is correct in configuration

---

### Issue: "No Subscriptions Found"

**Symptoms:**
- Test Connection shows 0 accessible subscriptions
- Validation always returns "Available"

**Diagnosis:**
```bash
# List all role assignments
az role assignment list --assignee <principal-id> --all

# Check subscription IDs in config
cat src/settings/azurevalidationsettings.json | jq .SubscriptionIds
```

**Resolution:**
1. Verify subscription IDs are correct GUIDs
2. Assign Reader role on subscriptions
3. Remove `ManagementGroupId` if not needed
4. Test with single subscription first

---

### Issue: "Resource Graph Query Timeout"

**Symptoms:**
- Validation takes > 5 seconds
- Intermittent validation failures
- Large subscription with many resources

**Resolution:**
1. Reduce number of subscriptions in query
2. Increase cache duration to reduce queries
3. Use Management Group if possible (more efficient)
4. Consider pagination for large result sets
5. Contact support if persistent

---

### Issue: "CheckNameAvailability API Fails"

**Symptoms:**
- Global resources (Storage, KeyVault) show errors
- Scoped resources work fine
- API version errors

**Diagnosis:**
```bash
# Check if resource provider is registered
az provider show --namespace Microsoft.Storage --query registrationState

# Test CheckNameAvailability API manually
az rest --method post \
  --url "https://management.azure.com/subscriptions/<sub-id>/providers/Microsoft.Storage/checkNameAvailability?api-version=2023-01-01" \
  --body '{"name":"testname123","type":"Microsoft.Storage/storageAccounts"}'
```

**Resolution:**
1. Register resource providers: `az provider register --namespace Microsoft.Storage`
2. Wait for registration: `az provider show --namespace Microsoft.Storage`
3. Verify API versions are current (see code)
4. Fallback to Resource Graph if API unavailable

---

## Maintenance

### Regular Tasks

**Weekly:**
- [ ] Review admin logs for validation errors
- [ ] Check cache hit rate (should be > 50%)
- [ ] Monitor API throttling messages

**Monthly:**
- [ ] Review RBAC role assignments
- [ ] Check for expiring Service Principal secrets
- [ ] Update Azure SDK packages if new versions available
- [ ] Review conflict resolution patterns

**Quarterly:**
- [ ] Audit all Service Principals
- [ ] Rotate Service Principal secrets
- [ ] Review and update documentation
- [ ] Test disaster recovery procedures
- [ ] Performance testing and optimization

### Secret Rotation Procedure

**For Service Principal with Key Vault:**

1. **Create New Secret**
```bash
# Create new client secret in Azure AD
$newSecret = az ad sp credential reset \
  --id <client-id> \
  --append \
  --output json | ConvertFrom-Json

# Update Key Vault
az keyvault secret set \
  --vault-name naming-tool-kv \
  --name "naming-tool-client-secret" \
  --value $newSecret.password
```

2. **Test with New Secret**
- Click Test Connection in Admin UI
- Verify successful authentication
- Generate test names
- Verify validation works

3. **Remove Old Secret**
```bash
# List all credentials
az ad sp credential list --id <client-id>

# Delete old credential by key ID
az ad sp credential delete \
  --id <client-id> \
  --key-id <old-key-id>
```

4. **Document Rotation**
- Log in admin notes
- Update runbook
- Schedule next rotation (90 days)

---

### Performance Tuning

**Cache Optimization:**
- **High Traffic**: Increase cache duration to 10-15 minutes
- **Low Traffic**: Decrease to 1-2 minutes for fresher results
- **Dev/Test**: Disable cache for immediate validation

**Subscription Management:**
- **Small Org**: Query all subscriptions
- **Large Org**: Use Management Group scope
- **Multi-Tenant**: Separate configurations per tenant

**Conflict Strategy:**
- **High Automation**: AutoIncrement (fastest)
- **Manual Review**: NotifyOnly (allows review)
- **Strict Compliance**: Fail (enforces manual resolution)
- **Random Names**: SuffixRandom (always unique)

---

## Additional Resources

- [Security Guide](./AZURE_VALIDATION_SECURITY_GUIDE.md)
- [Implementation Plan](./AZURE_NAME_VALIDATION_PLAN.md)
- [API Documentation](./API_MIGRATION_PLAN.md)
- [Troubleshooting FAQ](https://github.com/mspnp/AzureNamingTool/wiki/Azure-Validation-FAQ)

---

*Document Version: 1.0*  
*Last Updated: January 2025*  
*Applies to: Azure Naming Tool v5.0.0+*

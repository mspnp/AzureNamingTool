# Azure Validation - Docker Deployment Wiki

---

## Overview

This wiki provides comprehensive step-by-step instructions for enabling Azure Tenant Name Validation in the Azure Naming Tool when running in Docker.

> **Note:** For complete feature documentation, see the [Azure Validation Wiki](AZURE-VALIDATION-WIKI).

---

## Prerequisites

✅ Docker Desktop or Docker Engine installed  
✅ Azure Naming Tool running in Docker with a persistent volume  
✅ Azure subscription with appropriate permissions  

---

## Step 1: Create Service Principal

```bash
# Create a service principal with Reader permissions
az ad sp create-for-rbac \
  --name "naming-tool-sp" \
  --role "Reader" \
  --scopes "/subscriptions/<YOUR-SUBSCRIPTION-ID>" \
  --output json
```

**Save the output immediately - you'll need:**
- `appId` → Client ID
- `password` → Client Secret (cannot be retrieved later!)
- `tenant` → Tenant ID

---

## Step 2: Find Your Container and Volume

```bash
# Get container ID
docker ps

# Find your volume name
docker volume ls | grep naming

# Common volume names:
# - azurenamingtoolv5
# - azurenamingtool_settings
```

---

## Step 3: Configure Settings File

> **📝 Note for Upgrading Users:**  
> If you upgraded to v5.0.0, the file `azurevalidationsettings.json` already exists in your volume's `settings/` folder.  
> You just need to **edit** it to add your Service Principal credentials - no need to create it from scratch.

### Option A: Using Docker Desktop (Easiest)

1. Open **Docker Desktop**
2. Go to **Volumes** tab
3. Click on your naming tool volume
4. Click **Browse** or **Files**
5. Navigate to the `settings/` folder
6. **Edit the existing** `azurevalidationsettings.json` file (or create if missing)
7. Update with your Service Principal credentials (see configuration template below)
8. Save the file

### Option B: Using Command Line

```bash
# Replace these values with your actual credentials
export TENANT_ID="your-tenant-id-here"
export CLIENT_ID="your-client-id-here"
export CLIENT_SECRET="your-client-secret-here"
export SUBSCRIPTION_ID="your-subscription-id-here"
export CONTAINER_ID="your-container-id-here"

# Create/update the configuration file
cat > /tmp/azurevalidationsettings.json << EOF
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "${TENANT_ID}",
    "SubscriptionIds": ["${SUBSCRIPTION_ID}"],
    "ManagementGroupId": null,
    "ServicePrincipal": {
      "ClientId": "${CLIENT_ID}",
      "ClientSecret": "${CLIENT_SECRET}",
      "ClientSecretKeyVaultName": null
    },
    "KeyVault": null,
    "ConflictResolution": {
      "Strategy": "NotifyOnly",
      "MaxAttempts": 100,
      "IncrementPadding": 3,
      "IncludeWarnings": true
    },
    "Cache": {
      "Enabled": true,
      "DurationMinutes": 5
    }
  }
]
EOF

# Copy to container (this will overwrite the existing file)
docker cp /tmp/azurevalidationsettings.json ${CONTAINER_ID}:/app/settings/azurevalidationsettings.json

# Clean up
rm /tmp/azurevalidationsettings.json
```

---

## Step 4: Configuration Template

Copy this template and replace the values:

```json
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "YOUR-TENANT-ID",
    "SubscriptionIds": [
      "YOUR-SUBSCRIPTION-ID-1",
      "YOUR-SUBSCRIPTION-ID-2"
    ],
    "ManagementGroupId": null,
    "ServicePrincipal": {
      "ClientId": "YOUR-CLIENT-ID",
      "ClientSecret": "YOUR-CLIENT-SECRET",
      "ClientSecretKeyVaultName": null
    },
    "KeyVault": null,
    "ConflictResolution": {
      "Strategy": "NotifyOnly",
      "MaxAttempts": 100,
      "IncrementPadding": 3,
      "IncludeWarnings": true
    },
    "Cache": {
      "Enabled": true,
      "DurationMinutes": 5
    }
  }
]
```

### Critical Requirements

⚠️ **File must be a JSON array** - wrapped in `[` and `]`  
⚠️ **File name** - `azurevalidationsettings.json`  
⚠️ **String values required for enums:**
- `"AuthMode": "ServicePrincipal"` (use string name, not numeric value)
- `"Strategy": "NotifyOnly"` (use string name, not numeric value)

### Enum Reference

> **Note:** In configuration files, always use the string values shown in quotes (e.g., `"ServicePrincipal"`, `"NotifyOnly"`). Numeric values are shown for reference only.

| Field      | String Values (use in config) | Numeric Mapping (for reference) |
|------------|------------------------------|----------------------------------|
| `AuthMode` | `"ManagedIdentity"`, `"ServicePrincipal"` | `0` = ManagedIdentity, `1` = ServicePrincipal |
| `Strategy` | `"NotifyOnly"`, `"AutoIncrement"`, `"Fail"` | `0` = NotifyOnly, `1` = AutoIncrement, `2` = Fail |

---

## Step 5: Restart Container

```bash
docker restart <container-id>

# Wait for startup
sleep 5
```

---

## Step 6: Verify Configuration

```bash
# Check logs for errors
docker logs <container-id> --tail 50

# Look for authentication errors
docker logs <container-id> 2>&1 | grep -i "failed to authenticate"

# Verify file content
docker exec <container-id> cat /app/settings/azurevalidationsettings.json
```

**Success:** No "Client secret not found" or authentication errors  
**Failure:** See [Troubleshooting](#troubleshooting) below

---

## Step 7: Test in UI

1. Open Azure Naming Tool: `http://localhost:8080` (or your configured port)
2. Navigate to **Admin** → **Configuration** → **Site Configuration**
3. Scroll to **Azure Tenant Name Validation**
4. Click **Test Connection**
5. You should see:
   - ✅ Authentication: Success
   - ✅ Subscriptions: [Your subscriptions listed]
   - ✅ Resource Graph: Accessible

---

## Troubleshooting

### Error: "Client secret not found in Key Vault or configuration"

**Problem:** The `ClientSecret` field is missing or empty.

**Solution:**
```bash
# Verify ClientSecret is in the file
docker exec <container-id> cat /app/settings/azurevalidationsettings.json | grep "ClientSecret"

# Should show: "ClientSecret": "Q2Z8Q~..."
# If empty or missing, recreate the configuration file
```

---

### Error: "The JSON value could not be converted to AuthenticationMode"

**Problem:** `AuthMode` has an invalid value.

**Fix:** Ensure `"AuthMode": "ServicePrincipal"` or `"AuthMode": "ManagedIdentity"` (use valid string enum names)

---

### Error: "Failed to authenticate to Azure"

**Possible Causes:**

1. **Wrong credentials** - Verify Service Principal details:
   ```bash
   az ad sp show --id <client-id>
   ```

2. **Expired secret** - Reset the credential:
   ```bash
   az ad sp credential reset --id <client-id>
   ```

3. **Wrong Tenant ID** - Verify:
   ```bash
   az account show --query tenantId
   ```

---

### Configuration Not Loading

**Solution:**
```bash
# Ensure file exists
docker exec <container-id> ls -la /app/settings/

# Check file permissions
docker exec <container-id> ls -l /app/settings/azurevalidationsettings.json

# Restart container
docker restart <container-id>
```

---

## Complete Example

Here's a complete working example with placeholder values:

```json
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "YOUR-TENANT-ID-HERE",
    "SubscriptionIds": [
      "YOUR-SUBSCRIPTION-ID-1",
      "YOUR-SUBSCRIPTION-ID-2"
    ],
    "ManagementGroupId": null,
    "ServicePrincipal": {
      "ClientId": "YOUR-CLIENT-ID-HERE",
      "ClientSecret": "YOUR-CLIENT-SECRET-HERE",
      "ClientSecretKeyVaultName": null
    },
    "KeyVault": null,
    "ConflictResolution": {
      "Strategy": "NotifyOnly",
      "MaxAttempts": 100,
      "IncrementPadding": 3,
      "IncludeWarnings": true
    },
    "Cache": {
      "Enabled": true,
      "DurationMinutes": 5
    }
  }
]
```

---

## Security Best Practices

1. **Protect the configuration file** - Contains sensitive secrets
2. **Rotate secrets regularly** - Every 6-12 months
3. **Use minimum permissions** - Reader role only
4. **Consider Azure Key Vault** - For production deployments
5. **Monitor access logs** - Track who accesses Docker volumes

---

## Getting Help

- **Full Documentation:** [AZURE_VALIDATION_WIKI](AZURE-VALIDATION-WIKI)
- **GitHub Issues:** [Azure Naming Tool Issues](https://github.com/mspnp/AzureNamingTool/issues)
- **Docker Troubleshooting:** See [Docker Deployment Configuration](wiki/AZURE_VALIDATION_WIKI.md#docker-deployment-configuration)

---

## What's Next?

✅ Enable Azure Validation in name generation  
✅ Configure conflict resolution strategy  
✅ Set up caching for better performance  
✅ Monitor validation success rates  

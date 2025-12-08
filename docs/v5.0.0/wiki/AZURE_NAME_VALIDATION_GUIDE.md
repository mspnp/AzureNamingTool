# Azure Validation Feature

---

## Quick Start Guide

Choose your authentication method and follow the steps below to enable Azure Tenant Name Validation:

### Which Authentication Method Should I Use?

| Factor | Managed Identity | Service Principal |
|--------|------------------|-------------------|
| **Best For** | Azure-hosted deployments | Docker, on-premises, or development |
| **Security** | ✅ No credentials to manage | ⚠️ Requires secret management |
| **Setup Complexity** | ✅ Simple (UI only) | ⚠️ Moderate (requires file editing for Docker) |
| **Azure Requirement** | Must run in Azure | Can run anywhere |
| **Recommended Use** | Production Azure deployments | Docker containers, local testing |

---

### 🔹 Option 1: Managed Identity (Recommended for Azure-hosted deployments)

**Prerequisites:**
- Azure Naming Tool deployed in Azure (App Service, Container App, or VM)

**Steps:**

1. **In Azure Portal:**
   - Enable Managed Identity on your Azure resource (App Service/Container App/VM)
   - Navigate to **Access control (IAM)** on your subscription(s)
   - Click **Add role assignment**
   - Select **Reader** role
   - Assign to your Managed Identity

2. **In Azure Naming Tool Admin UI:**
   - Navigate to **Admin** → **Site Settings**
   - Scroll to **Azure Tenant Name Validation** section
   - Click **Enable** toggle
   - Select **Managed Identity** as authentication mode
   - Click **Test Connection** to verify Azure connectivity
   - Select your subscription(s) from the dropdown
   - Configure validation options:
     - **Conflict Resolution Strategy** (NotifyOnly, AutoIncrement, or Fail)
     - **Cache duration** (default: 5 minutes)
   - Click **Save Azure Validation Settings**

3. **Verify:**
   - Generate a resource name
   - Observe validation status (Available/In Use)

---

### 🔹 Option 2: Service Principal (Required for Docker/on-premises deployments)

**Prerequisites:**
- Azure subscription with permissions to create Service Principals
- Access to Azure CLI or Azure Portal

**Steps:**

1. **Create Service Principal in Azure:**
   ```bash
   az ad sp create-for-rbac \
     --name "naming-tool-sp" \
     --role "Reader" \
     --scopes "/subscriptions/YOUR-SUBSCRIPTION-ID"
   ```
   - Save the output: `appId` (Client ID), `password` (Client Secret), `tenant` (Tenant ID)

2. **For Docker Deployments - Configure Settings File:**
   - Edit `azurevalidationsettings.json` in your Docker volume's `settings/` folder
   - Update with your Service Principal credentials:
     ```json
     {
       "AuthMode": "ServicePrincipal",
       "TenantId": "YOUR-TENANT-ID",
       "SubscriptionIds": ["YOUR-SUBSCRIPTION-ID"],
       "ServicePrincipal": {
         "ClientId": "YOUR-CLIENT-ID",
         "ClientSecret": "YOUR-CLIENT-SECRET"
       }
     }
     ```
   - Restart container
   - **See [Docker Wiki Guide](AZURE-VALIDATION-DOCKER-WIKI) for detailed instructions**

3. **For Non-Docker Deployments - Use Admin UI:**
   - Navigate to **Admin** → **Site Settings**
   - Scroll to **Azure Tenant Name Validation** section
   - Click **Enable** toggle
   - Select **Service Principal** as authentication mode
   - Enter your Service Principal details:
     - Tenant ID
     - Client ID
     - Client Secret (stored in configuration file)
   - Click **Test Connection** to verify credentials
   - Select your subscription(s) from the dropdown
   - Configure validation options:
     - **Conflict Resolution Strategy** (NotifyOnly, AutoIncrement, or Fail)
     - **Cache duration** (default: 5 minutes)
   - Click **Save Azure Validation Settings**
   - ⚠️ **Important:** Manually add `ClientSecret` to `/settings/azurevalidationsettings.json` (Admin UI doesn't save secrets)

4. **Verify:**
   - Generate a resource name
   - Observe validation status (Available/In Use)

---

## Overview

The Azure Validation feature enables the Azure Naming Tool to validate generated resource names against your actual Azure tenant in real-time. This ensures that:

- **Names are unique** - Prevents naming conflicts before deployment
- **Resources don't exist** - Checks if a resource with the same name already exists
- **Compliance is maintained** - Validates against your organization's actual Azure environment
- **Deployment success** - Reduces deployment failures due to naming conflicts

### How It Works

When you generate a resource name with Azure Validation enabled:

1. **Name Generation** - The tool generates a name based on your naming convention
2. **Azure Query** - The tool queries your Azure tenant using Azure Resource Graph or CheckNameAvailability API
3. **Validation Result** - Returns whether the name exists in your Azure environment
4. **Conflict Resolution** - Optionally auto-increments the instance number if a conflict is found
5. **Metadata Returned** - Provides validation metadata including resource IDs if found

### Validation Methods

The Azure Validation feature uses two methods depending on resource type:

| Method | Used For | Description |
|--------|----------|-------------|
| **Azure Resource Graph** | Most resources | Queries existing resources across subscriptions using KQL |
| **CheckNameAvailability API** | Globally unique resources | Uses Azure Management API for storage accounts, Key Vaults, etc. |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Naming Tool                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  1. User Generates Name (e.g., "vm-prod-eus2-app-001")│  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  2. Azure Validation Service                          │  │
│  │     - Checks if validation enabled                    │  │
│  │     - Authenticates to Azure                          │  │
│  │     - Checks cache (if enabled)                       │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  3. Query Azure Tenant                                │  │
│  │     ┌─────────────────┬─────────────────┐             │  │
│  │     │ Resource Graph  │ Check Name API  │             │  │
│  │     │ (Most Resources)│ (Global Scope)  │             │  │
│  │     └─────────────────┴─────────────────┘             │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  4. Return Validation Result                          │  │
│  │     - Exists: true/false                              │  │
│  │     - Resource IDs (if found)                         │  │
│  │     - Validation metadata                             │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                   ┌─────────────┐
                   │ Azure Tenant│
                   │ (Your Subs) │
                   └─────────────┘
```

---

## Authentication Methods

Azure Validation supports two authentication methods:

### 1. Managed Identity (Recommended)

**Best for:** Azure-hosted deployments (App Service, Container Apps, VMs, AKS)

**Advantages:**
- ✅ No secrets to manage
- ✅ Automatic credential rotation
- ✅ More secure (credentials never leave Azure)
- ✅ Easier to set up
- ✅ Integrated with Azure RBAC

**Disadvantages:**
- ❌ Only works when hosted in Azure
- ❌ Cannot be used for local development

### 2. Service Principal

**Best for:** On-premises deployments, local development, non-Azure hosting

**Advantages:**
- ✅ Works anywhere (Azure, on-prem, local)
- ✅ Good for CI/CD pipelines
- ✅ Can be used in development environments

**Disadvantages:**
- ❌ Requires managing secrets
- ❌ Secrets need periodic rotation
- ❌ Higher security risk if secrets are compromised

---

## Configuration Guide

### Prerequisites

Before configuring Azure Validation, ensure you have:

- [ ] Azure subscription(s) with resources to validate against
- [ ] Appropriate permissions to create identities or service principals
- [ ] Access to the Azure Naming Tool Admin section

---

## Option 1: Managed Identity Setup

### Step 1: Enable Managed Identity

Choose the appropriate method based on your hosting environment:

#### **Azure App Service**

```bash
# Enable System-Assigned Managed Identity
az webapp identity assign \
  --name <your-app-name> \
  --resource-group <your-resource-group>

# Get the Principal ID (you'll need this for RBAC)
az webapp identity show \
  --name <your-app-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure Container Apps**

```bash
# Enable System-Assigned Managed Identity
az containerapp identity assign \
  --name <your-containerapp-name> \
  --resource-group <your-resource-group>

# Get the Principal ID
az containerapp identity show \
  --name <your-containerapp-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure VM**

```bash
# Enable System-Assigned Managed Identity
az vm identity assign \
  --name <your-vm-name> \
  --resource-group <your-resource-group>

# Get the Principal ID
az vm identity show \
  --name <your-vm-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure Kubernetes Service (AKS)**

```bash
# Create a user-assigned managed identity
az identity create \
  --name naming-tool-identity \
  --resource-group <your-resource-group>

# Get the identity details
az identity show \
  --name naming-tool-identity \
  --resource-group <your-resource-group> \
  --query "{clientId: clientId, principalId: principalId}" \
  --output json

# Configure your pod to use the identity (via Azure Workload Identity or aad-pod-identity)
```

### Step 2: Assign RBAC Permissions

The managed identity needs **Reader** permissions to query Azure resources.

#### **Single Subscription**

```bash
# Assign Reader role at subscription scope
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

#### **Multiple Subscriptions**

```bash
# Repeat for each subscription
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-1>"

az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-2>"
```

#### **Management Group (Recommended for Large Organizations)**

```bash
# Assign Reader role at management group scope (inherited by all subscriptions)
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/providers/Microsoft.Management/managementGroups/<management-group-id>"
```

#### **Verify Role Assignment**

```bash
# Check role assignments for the managed identity
az role assignment list \
  --assignee <principal-id-from-step-1> \
  --output table
```

### Step 3: Configure in Azure Naming Tool

1. Navigate to **Admin** → **Site Configuration** → **Azure Validation**
2. Click **Edit Configuration**
3. Set the following:
   - **Enable Azure Validation**: ✅ Checked
   - **Authentication Mode**: `Managed Identity`
   - **Tenant ID**: Your Azure AD tenant ID
   - **Subscription IDs**: Add all subscriptions to query (comma-separated or one per line)
4. Click **Save**

#### Finding Your Tenant ID

```bash
# Get your tenant ID
az account show --query tenantId --output tsv
```

#### Getting Subscription IDs

```bash
# List all subscriptions you have access to
az account list --query "[].{Name:name, SubscriptionId:id}" --output table

# Get current subscription ID
az account show --query id --output tsv
```

### Step 4: Test the Configuration

1. Navigate to **Generate** page
2. Generate a name for any resource type
3. Enable **Azure Validation** toggle
4. Generate the name
5. Check the validation result below the generated name

**Expected Result:**
```
✅ Validation Performed: Yes
✅ Exists in Azure: No (or Yes with Resource IDs)
✅ Timestamp: [current time]
```

---

## Option 2: Service Principal Setup

### Step 1: Create Service Principal

```bash
# Create a service principal with Reader role
az ad sp create-for-rbac \
  --name "naming-tool-sp" \
  --role "Reader" \
  --scopes "/subscriptions/<subscription-id>" \
  --output json

# Save the output - YOU WILL NEED THESE VALUES:
# {
#   "appId": "00000000-0000-0000-0000-000000000000",       # CLIENT_ID
#   "displayName": "naming-tool-sp",
#   "password": "your-secret-here",                        # CLIENT_SECRET
#   "tenant": "00000000-0000-0000-0000-000000000000"       # TENANT_ID
# }
```

⚠️ **IMPORTANT:** Save the `password` (client secret) immediately - it cannot be retrieved later!

### Step 2: Assign Additional Subscriptions (if needed)

```bash
# Get the service principal's App ID (if you didn't save it)
az ad sp list --display-name "naming-tool-sp" --query "[0].appId" --output tsv

# Assign to additional subscriptions
az role assignment create \
  --assignee <app-id-from-above> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-2>"
```

### Step 3: Store Client Secret

#### **Option A: Direct Configuration (Not Recommended for Production)**

Store the client secret directly in the Azure Naming Tool configuration.

⚠️ **Security Risk:** Secret is stored in the database/configuration file.

#### **Option B: Azure Key Vault (Recommended)**

Store the client secret in Azure Key Vault for enhanced security.

```bash
# Create a Key Vault (if you don't have one)
az keyvault create \
  --name <your-keyvault-name> \
  --resource-group <your-resource-group> \
  --location <location>

# Store the client secret
az keyvault secret set \
  --vault-name <your-keyvault-name> \
  --name "naming-tool-client-secret" \
  --value "<client-secret-from-step-1>"

# Grant the naming tool's managed identity access to Key Vault
az keyvault set-policy \
  --name <your-keyvault-name> \
  --object-id <naming-tool-managed-identity-principal-id> \
  --secret-permissions get

# Get the Key Vault URI
az keyvault show \
  --name <your-keyvault-name> \
  --query properties.vaultUri \
  --output tsv
```

### Step 4: Configure Basic Settings in Azure Naming Tool

1. Navigate to **Admin** → **Site Configuration** → **Azure Validation**
2. Click **Edit Configuration**
3. Set the following:
   - **Enable Azure Validation**: ✅ Checked
   - **Authentication Mode**: `Service Principal`
   - **Tenant ID**: `<tenant-id-from-step-1>`
   - **Subscription IDs**: Add all subscriptions to query
   - **Client ID**: `<appId-from-step-1>`
4. Click **Save**

⚠️ **Important:** The Admin UI does NOT save the Client Secret for security reasons. You must manually add it to the configuration file in Step 5.

### Step 5: Manually Add Client Secret to Configuration File

The `ClientSecret` must be added directly to the configuration file:

#### **For Non-Docker Deployments:**

1. Locate the configuration file:
   - **FileSystem storage**: `settings/azurevalidationsettings.json`
   - **SQLite storage**: Use the Admin UI (Client Secret is encrypted in database)

2. Edit the file and add the `ClientSecret`:

```json
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "your-tenant-id",
    "SubscriptionIds": ["your-subscription-id"],
    "ServicePrincipal": {
      "ClientId": "your-client-id",
      "ClientSecret": "YOUR-CLIENT-SECRET-HERE",
      "ClientSecretKeyVaultName": null
    },
    ...
  }
]
```

3. Save the file and restart the application

#### **For Docker Deployments:**

See the [Docker Deployment Configuration](#docker-deployment-configuration) section below for detailed steps.

#### **For Key Vault (Recommended for Production):**

If using Azure Key Vault to store the secret:

1. Store the secret in Key Vault (see Step 3 above)
2. Configure Key Vault settings in the Admin UI:
   - **Key Vault URI**: `https://<your-keyvault-name>.vault.azure.net/`
   - **Secret Name**: `naming-tool-client-secret`
3. The application will retrieve the secret from Key Vault at runtime

### Step 6: Test the Configuration

Same as Managed Identity Step 4 above.

---

## Docker Deployment Configuration

**Important:** Docker deployments require special configuration steps because:
1. The Admin UI does NOT save the `ClientSecret` for security reasons
2. Configuration settings must be manually edited in the persistent volume
3. Settings must be properly formatted as a JSON array

⚠️ **You MUST manually edit the configuration file** - The Admin UI alone is insufficient for Docker deployments.

### Prerequisites for Docker

- [ ] Docker Desktop or Docker Engine installed
- [ ] Azure Naming Tool Docker container running with a volume for settings
- [ ] Service Principal created (see [Option 2: Service Principal Setup](#option-2-service-principal-setup))
- [ ] Client ID, Client Secret, Tenant ID, and Subscription IDs ready

### Step 1: Locate the Settings Volume

The Azure Naming Tool uses a Docker volume to persist configuration. Find your volume:

```bash
# List all volumes
docker volume ls

# Inspect the naming tool volume (usually named azurenamingtoolv<X>)
docker volume inspect <volume-name>

# Common volume names:
# - azurenamingtoolv5
# - azurenamingtool_settings
# - naming-tool-data
```

**Volume Mount Point:**
- **Windows**: `\\wsl$\docker-desktop-data\version-pack-data\community\docker\volumes\<volume-name>\_data`
- **macOS**: `/var/lib/docker/volumes/<volume-name>/_data`
- **Linux**: `/var/lib/docker/volumes/<volume-name>/_data`

**Accessing via Docker Desktop:**
1. Open Docker Desktop
2. Go to **Volumes** tab
3. Click on your naming tool volume (e.g., `azurenamingtoolv5`)
4. Click **Browse** or **Files** to access the volume contents

### Step 2: Create/Update Configuration File

You need to update the file `azurevalidationsettings.json` in the `settings/` folder within the volume.

#### Important File Details

- **File Name**: `azurevalidationsettings.json`
- **Location**: `<volume-mount-point>/settings/azurevalidationsettings.json`
- **Format**: JSON array (wrapped in `[]`)
- **Encoding**: UTF-8

#### Configuration Template

Create or update the file with the following content:

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

#### Configuration Values Explained

| Field | Value | Description |
|-------|-------|-------------|
| `Id` | `1` | Always use `1` (singleton configuration) |
| `Enabled` | `true` | Set to `true` to enable Azure validation |
| `AuthMode` | `1` | **Must be numeric:** `0` = Managed Identity, `1` = Service Principal |
| `TenantId` | GUID string | Your Azure AD tenant ID |
| `SubscriptionIds` | Array of strings | List of subscription IDs to query for validation |
| `ServicePrincipal.ClientId` | GUID string | Service Principal Application (client) ID |
| `ServicePrincipal.ClientSecret` | String | Service Principal client secret (password) |
| `ConflictResolution.Strategy` | `0`, `1`, or `2` | **Must be numeric:** `0` = NotifyOnly, `1` = AutoIncrement, `2` = Fail |

⚠️ **Critical:** All enum values (`AuthMode`, `Strategy`) **must be numbers**, not strings!

#### Example Configuration

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

### Step 3: Update the Configuration File

#### Option A: Using Docker Desktop (Recommended)

1. Open **Docker Desktop**
2. Navigate to **Volumes** → `<your-naming-tool-volume>`
3. Click **Browse** or **Files**
4. Navigate to `settings/` folder
5. **Edit the existing** `azurevalidationsettings.json` file
   > **Note:** The file already exists if you upgraded to v5.0.0 - just update it with your credentials
6. Update with your Service Principal credentials
7. Save the file

#### Option B: Using Docker Command Line

```bash
# Get your container ID
docker ps

# Create/update the configuration file locally
cat > azurevalidationsettings.json << 'EOF'
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "YOUR-TENANT-ID",
    "SubscriptionIds": ["YOUR-SUBSCRIPTION-ID"],
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
EOF

# Edit the file with your values
nano azurevalidationsettings.json  # or vim, code, notepad, etc.

# Copy to container (this will overwrite the existing file)
docker cp azurevalidationsettings.json <container-id>:/app/settings/azurevalidationsettings.json

# Clean up local file
rm azurevalidationsettings.json
```

#### Option C: Using Docker Exec (PowerShell)

```powershell
# Get container ID
docker ps

# Create the configuration with your values (update the values first!)
@'
[
  {
    "Id": 1,
    "Enabled": true,
    "AuthMode": "ServicePrincipal",
    "TenantId": "YOUR-TENANT-ID",
    "SubscriptionIds": ["YOUR-SUBSCRIPTION-ID"],
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
'@ | docker exec -i <container-id> tee /app/settings/azurevalidationsettings.json > $null
```

### Step 4: Verify the Configuration

```bash
# Verify the file exists and has correct content
docker exec <container-id> cat /app/settings/azurevalidationsettings.json

# Check that ClientSecret is present (security note: this will display the secret!)
docker exec <container-id> cat /app/settings/azurevalidationsettings.json | grep "ClientSecret"
```

**Expected Output:**
- Should show your JSON configuration
- `ClientSecret` field should contain your secret value
- File should be a valid JSON array `[{...}]`

### Step 5: Restart the Container

The configuration is loaded when the container starts, so you must restart:

```bash
# Restart the container
docker restart <container-id>

# Or restart using Docker Desktop:
# 1. Go to Containers tab
# 2. Find your naming tool container
# 3. Click the restart icon
```

### Step 6: Verify Configuration Loaded

```bash
# Wait a few seconds for the container to start, then check logs
docker logs <container-id> --tail 50

# Look for successful authentication messages
docker logs <container-id> 2>&1 | grep -i "azure"
```

**Success indicators:**
- ✅ No "Client secret not found" errors
- ✅ No "Failed to authenticate to Azure" errors
- ✅ May see "Azure ARM client authenticated using ServicePrincipal" (in debug mode)

**Common errors:**
- ❌ "Client secret not found in Key Vault or configuration" - ClientSecret is missing or empty
- ❌ "The JSON value could not be converted to AuthenticationMode/ConflictStrategy" - AuthMode must be a valid enum value ("ManagedIdentity" or "ServicePrincipal")
- ❌ "Failed to authenticate to Azure" - Check ClientId, ClientSecret, or TenantId values

### Step 7: Test Azure Validation

1. Open the Azure Naming Tool in your browser (e.g., `http://localhost:8080` or `http://localhost:8081`)
2. Navigate to **Admin** → **Configuration** → **Site Configuration**
3. Scroll to **Azure Tenant Name Validation** section
4. You should see:
   - **Status**: Enabled
   - **Authentication Mode**: Service Principal
   - **Subscriptions**: Your configured subscription IDs
5. Click **Test Connection** button
6. You should see:
   - ✅ Authentication: Success
   - ✅ Subscriptions accessible: [list of subscriptions]
   - ✅ Resource Graph access: Yes

### Step 8: Test Name Generation with Validation

1. Navigate to **Generate** page
2. Select a resource type (e.g., Virtual Machine)
3. Fill in the required components
4. Enable the **Azure Validation** toggle
5. Click **Generate**
6. Check the validation result:

```
Generated Name: vm-prod-eus2-app-001

Azure Validation Results:
✅ Validation Performed: Yes
✅ Exists in Azure: No
✅ Validation Timestamp: 2025-11-10 15:30:00 UTC
```

---

## Docker Troubleshooting

### Issue: "Client secret not found in Key Vault or configuration"

**Cause:** The `ClientSecret` field is missing or empty in the configuration file.

**Solution:**
1. Verify the file `/app/settings/azurevalidationsettings.json` exists in the container
2. Check that `ServicePrincipal.ClientSecret` is not empty or null
3. Ensure the file is valid JSON and properly formatted as an array `[{...}]`

```bash
# Verify the file contents
docker exec <container-id> cat /app/settings/azurevalidationsettings.json | grep "ClientSecret"
```

### Issue: "The JSON value could not be converted to AuthenticationMode/ConflictStrategy"

**Cause:** The `AuthMode` field is a string (e.g., `"ServicePrincipal"`) instead of a number.

**Solution:** Change `AuthMode` to the numeric value:
- Use `1` for Service Principal (not `"ServicePrincipal"`)
- Use `0` for Managed Identity (not `"ManagedIdentity"`)

```json
"AuthMode": "ServicePrincipal",  // ✅ Correct (use string enum name)
"AuthMode": "ServicePrincipal",  // ❌ Wrong
```

### Issue: Configuration Changes Not Taking Effect

**Cause:** The container hasn't been restarted after updating the configuration.

**Solution:**
```bash
# Always restart after changing configuration
docker restart <container-id>

# Wait for startup
sleep 5

# Check logs for errors
docker logs <container-id> --tail 50
```

### Issue: "Failed to authenticate to Azure"

**Causes & Solutions:**

1. **Invalid Service Principal credentials**
   - Verify `ClientId`, `ClientSecret`, and `TenantId` are correct
   - Check if the service principal still exists: `az ad sp show --id <client-id>`
   - Try creating a new service principal

2. **Service Principal secret expired**
   - Service principal secrets expire (default: 1-2 years)
   - Create a new secret: `az ad sp credential reset --id <client-id>`
   - Update the configuration with the new secret

3. **Wrong Tenant ID**
   - Verify your tenant ID: `az account show --query tenantId`
   - Ensure it matches the `TenantId` in your configuration

4. **Network/Firewall issues**
   - Ensure the container can reach Azure endpoints
   - Test: `docker exec <container-id> curl -I https://management.azure.com`

### Issue: Container Can't Find the Configuration File

**Cause:** The volume is not properly mounted or the file is in the wrong location.

**Solution:**
1. Check volume mounts: `docker inspect <container-id> | grep Mounts -A 20`
2. Verify settings folder exists: `docker exec <container-id> ls -la /app/settings/`
3. Recreate the container with proper volume mount:

```bash
docker run -d \
  -p 8080:80 \
  -v azurenamingtoolv5:/app/settings \
  --name naming-tool \
  azurenamingtool:latest
```

### Issue: Permission Denied Reading Configuration File

**Cause:** File permissions are incorrect in the volume.

**Solution:**
```bash
# Fix permissions in container
docker exec <container-id> chmod 644 /app/settings/azurevalidationsettings.json

# Restart container
docker restart <container-id>
```

### Issue: Settings Appear in UI But Validation Fails

**Cause:** The UI may be showing different settings than what the backend is using.

**Solution:**
1. Check the actual file being loaded:
   ```bash
   docker exec <container-id> cat /app/settings/azurevalidationsettings.json
   ```
2. Verify it's formatted as an **array** with square brackets `[{...}]`
3. Ensure all required fields are present and not null
4. Restart the container

### Debugging Commands

```bash
# Check container status
docker ps -a

# View recent logs
docker logs <container-id> --tail 100

# Search logs for errors
docker logs <container-id> 2>&1 | grep -i "error\|fail"

# Search logs for Azure-related messages
docker logs <container-id> 2>&1 | grep -i "azure"

# Check file system in container
docker exec <container-id> find /app -name "*azurevalidation*"

# View settings folder contents
docker exec <container-id> ls -la /app/settings/

# Verify JSON syntax
docker exec <container-id> cat /app/settings/azurevalidationsettings.json | python -m json.tool
```

### Security Best Practices for Docker

1. **Use Docker Secrets (Docker Swarm/Kubernetes)**
   ```bash
   # Create a secret
   echo "YOUR-CLIENT-SECRET" | docker secret create naming-tool-secret -
   
   # Update your service to use the secret
   # Then reference it in your application
   ```

2. **Use Environment Variables**
   ```bash
   docker run -d \
     -e AZURE_CLIENT_SECRET=<secret> \
     -e AZURE_CLIENT_ID=<client-id> \
     -e AZURE_TENANT_ID=<tenant-id> \
     azurenamingtool:latest
   ```
   Note: Requires application code changes to support environment variables

3. **Limit Container Permissions**
   ```bash
   docker run -d \
     --read-only \
     --tmpfs /tmp \
     --security-opt=no-new-privileges:true \
     azurenamingtool:latest
   ```

4. **Regular Secret Rotation**
   - Rotate service principal secrets every 6-12 months
   - Use Azure Key Vault for automatic rotation
   - Document the rotation process

5. **Audit Container Access**
   - Limit who can access Docker volumes
   - Use container registry with authentication
   - Monitor container logs for suspicious activity

---

## Advanced Configuration

### Conflict Resolution Strategies

When a name already exists in Azure, the tool can handle it in different ways:

| Strategy | Behavior | Use Case |
|----------|----------|----------|
| **Notify Only** (Default) | Returns name with a warning | User decides what to do |
| **Auto Increment** | Automatically increments instance number | Automatic unique name generation |
| **Fail** | Returns an error | Strict naming enforcement |
| **Suffix Random** | Adds random characters | When instance numbers aren't used |

**Example - Auto Increment:**
```
Requested: vm-prod-eus2-app-001
Exists in Azure: vm-prod-eus2-app-001
Auto-incremented: vm-prod-eus2-app-002
Exists in Azure: vm-prod-eus2-app-002
Auto-incremented: vm-prod-eus2-app-003
✅ Available: vm-prod-eus2-app-003
```

### Cache Settings

Validation results are cached to improve performance and reduce Azure API calls.

**Default Settings:**
- **Enabled**: Yes
- **Duration**: 5 minutes

**Configuration:**
```json
{
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 5
  }
}
```

**Considerations:**
- ✅ Reduces Azure Resource Graph query costs
- ✅ Improves response time for repeated queries
- ⚠️ May return stale results for recently created/deleted resources
- 💡 Use shorter durations (1-2 minutes) for highly dynamic environments

### Resource Type Exclusions

Exclude specific resource types from validation:

**Use Cases:**
- Resources not yet supported by Azure Resource Graph
- Resources you don't want to validate
- Performance optimization

**Example Configuration:**
```
Excluded Resource Types:
- Microsoft.Network/trafficManagerProfiles
- Microsoft.Cdn/profiles
```

### Subscription Filtering

**Best Practices:**
- ✅ Include only subscriptions where you deploy resources
- ✅ Use Management Groups for large organizations
- ⚠️ More subscriptions = longer query time
- 💡 Group subscriptions by environment (dev, test, prod)

---

## Troubleshooting

### Common Issues

#### 1. "Failed to authenticate to Azure"

**Possible Causes:**
- Managed Identity not enabled
- Service Principal credentials incorrect
- Key Vault access denied

**Solutions:**
```bash
# Verify managed identity exists
az webapp identity show --name <app-name> --resource-group <rg-name>

# Verify service principal exists
az ad sp show --id <client-id>

# Test Key Vault access
az keyvault secret show --vault-name <vault-name> --name <secret-name>
```

#### 2. "Validation performed but no results"

**Possible Causes:**
- No Reader role assigned
- Subscription IDs incorrect
- Resource doesn't exist in specified subscriptions

**Solutions:**
```bash
# Verify role assignments
az role assignment list --assignee <principal-id> --output table

# Verify subscription IDs
az account list --query "[].{Name:name, ID:id}" --output table

# Test manual query
az graph query -q "Resources | where type == 'microsoft.compute/virtualmachines' | project name"
```

#### 3. "Rate limiting or quota errors"

**Possible Causes:**
- Too many Azure Resource Graph queries
- Cache disabled
- High-frequency validation requests

**Solutions:**
- Enable caching
- Increase cache duration
- Reduce validation frequency
- Use batch validation (API)

#### 4. "Global scope resources not validated correctly"

**Possible Causes:**
- CheckNameAvailability API permissions missing
- Resource provider not registered

**Solutions:**
```bash
# Register required resource providers
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.KeyVault

# Verify registration
az provider show --namespace Microsoft.Storage --query "registrationState"
```

---

## Security Best Practices

### Managed Identity (Recommended)

✅ **DO:**
- Use system-assigned managed identity when possible
- Assign least-privilege permissions (Reader only)
- Use management group scope for multi-subscription access
- Monitor and audit role assignments regularly

❌ **DON'T:**
- Grant more permissions than needed (e.g., Contributor)
- Share managed identities across multiple applications
- Use user-assigned identities unless required

### Service Principal

✅ **DO:**
- Store secrets in Azure Key Vault
- Rotate secrets regularly (every 90 days)
- Use separate service principals per environment
- Enable secret expiration dates
- Monitor service principal sign-ins

❌ **DON'T:**
- Store secrets in plain text
- Commit secrets to source control
- Use the same service principal for multiple apps
- Create secrets without expiration dates

### Network Security

✅ **DO:**
- Use Private Endpoints for Key Vault access
- Restrict Key Vault network access
- Enable Azure Defender for Key Vault

---

## Performance Optimization

### Caching Strategy

| Environment | Cache Duration | Rationale |
|-------------|----------------|-----------|
| Development | 1-2 minutes | Frequently changing resources |
| Testing | 5 minutes (default) | Moderate change frequency |
| Production | 10-15 minutes | Stable environment |

### Subscription Filtering

**Optimize query performance:**
```
✅ Good: 2-5 subscriptions per environment
⚠️ Acceptable: 6-10 subscriptions
❌ Poor: 10+ subscriptions (consider Management Groups)
```

### Resource Type Exclusions

Exclude resource types that are:
- Not deployed in your environment
- Not managed by your naming convention
- High-volume but low-priority for validation

---

## API Integration

### REST API Endpoint

```http
POST /api/v1/generate
Content-Type: application/json

{
  "resourceEnvironment": "prod",
  "resourceInstance": "001",
  "resourceLocation": "eastus2",
  "resourceProjAppSvc": "app",
  "resourceType": "virtualmachines",
  "validateAzure": true
}
```

### Response with Validation

```json
{
  "success": true,
  "resourceName": "vm-prod-eus2-app-001",
  "validation": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationTimestamp": "2025-11-03T10:30:00Z",
    "resourceIds": [],
    "cacheHit": false
  }
}
```

---

## Monitoring & Logging

### Admin Logs

Azure Validation operations are logged in the Admin Log:

**Log Entries:**
- Authentication successes/failures
- Validation queries performed
- Cache hits/misses
- Configuration changes
- Errors and warnings

**Accessing Logs:**
1. Navigate to **Admin** → **Admin Log**
2. Filter by "Azure Validation" or "INFORMATION"
3. Review timestamps and messages

### Application Insights (Optional)

If using Azure Application Insights:

**Key Metrics:**
- Validation request count
- Cache hit rate
- Query duration
- Authentication failures
- API throttling events

**Sample KQL Query:**
```kql
traces
| where message contains "Azure validation"
| summarize count() by bin(timestamp, 1h), severityLevel
| render timechart
```

---

## Migration from JSON to SQLite

If you configured Azure Validation in FileSystem mode (JSON files) and then migrated to SQLite:

### ✅ v5.0.0+ (Fixed)

Azure Validation settings are now included in the storage migration.

**What happens:**
1. You configure Azure Validation in FileSystem mode
2. You migrate to SQLite storage
3. ✅ Azure Validation settings are automatically migrated
4. ✅ No reconfiguration needed

### ❌ Pre-v5.0.0 (Bug)

Azure Validation settings were **not** migrated automatically.

**Workaround:**
1. Export configuration before migration (Admin → Configuration → Export)
2. Migrate to SQLite
3. Manually re-enter Azure Validation settings
4. Or: Rollback to JSON, upgrade to v5.0.0+, re-migrate

---

## FAQ

### Q: How do I configure Azure Validation in Docker?

**A:** Docker deployments require manual configuration file editing. See the detailed [Docker Deployment Configuration](#docker-deployment-configuration) section above. Key points:
- Edit `/app/settings/azurevalidationsettings.json`
- Use numeric values for `AuthMode` and `Strategy` enums
- Ensure the file is a JSON array `[{...}]`
- Restart the container after making changes

### Q: Do I need Azure Resource Graph permissions?

**A:** No explicit Resource Graph permissions are needed. The **Reader** role at subscription scope provides sufficient access to query Azure Resource Graph.

### Q: Can I use Azure Validation with on-premises deployments?

**A:** Yes, using Service Principal authentication. Managed Identity only works when hosted in Azure.

### Q: How much does Azure Resource Graph cost?

**A:** Azure Resource Graph has a [free tier](https://azure.microsoft.com/en-us/pricing/details/azure-resource-graph/) of 1,000 queries per tenant per month. Beyond that, queries cost $0.001 per query. Caching helps reduce query costs.

### Q: Can I validate names across multiple tenants?

**A:** No, Azure Validation is scoped to a single tenant. You would need separate configurations for each tenant.

### Q: What happens if Azure is unavailable?

**A:** Validation is skipped and the name is returned without validation metadata. The tool does not block name generation.

### Q: Can I validate custom resource types?

**A:** Yes, if the resource type exists in Azure Resource Graph or has a CheckNameAvailability API.

### Q: How do I know which resources are validated?

**A:** Check the validation metadata in the response. `ValidationPerformed: true` indicates validation was attempted.

### Q: Can I disable validation for specific resource types?

**A:** Yes, use the Resource Type Exclusions feature in the configuration.

---

## Support & Resources

### Documentation

- [Azure Resource Graph Overview](https://learn.microsoft.com/en-us/azure/governance/resource-graph/overview)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
- [Azure Service Principals](https://learn.microsoft.com/en-us/entra/identity-platform/app-objects-and-service-principals)
- [Azure Key Vault Secrets](https://learn.microsoft.com/en-us/azure/key-vault/secrets/about-secrets)

### Community

- [Azure Naming Tool GitHub Issues](https://github.com/mspnp/AzureNamingTool/issues)
- [Azure Naming Tool Discussions](https://github.com/mspnp/AzureNamingTool/discussions)

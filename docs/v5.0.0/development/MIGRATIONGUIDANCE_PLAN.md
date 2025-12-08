# SQLite Migration Guidance Plan

## Overview
This document outlines the comprehensive plan for implementing a **one-way SQLite migration** with **dual backup/restore support** that allows users to restore from either SQLite database backups or JSON configuration backups.

## Executive Summary

### The Approach
Users with existing Azure Naming Tool installations will upgrade and remain on JSON (FileSystem) storage by default. They can then **choose to migrate** to SQLite for improved performance and reliability. The migration is **one-way** (no automatic rollback), but users have **flexible restore options**:

1. **Pre-Migration**: Export JSON configuration as backup
2. **Migration**: Automatically transfers all JSON data to SQLite database
3. **Post-Migration Backup**: Download SQLite database file OR export as JSON
4. **Post-Migration Restore**: Upload SQLite database backup OR upload JSON configuration

### Key Benefits
✅ **Non-disruptive**: Existing users stay on JSON until they choose to migrate  
✅ **Safe**: Pre-migration backup required, JSON files preserved  
✅ **Flexible**: Two restore methods (database file or JSON import)  
✅ **Compatible**: Can restore from pre-migration JSON backups into SQLite  
✅ **Simple**: Clear UI that adapts to current storage provider  

### User Experience Flow
```
Existing Installation (JSON)
    ↓
[User Upgrades App]
    ↓
Still Using JSON Files ← Persistent storage preserved
    ↓
[Admin Page: "Migrate to SQLite" option visible]
    ↓
[User clicks "Migrate to SQLite"]
    ↓
Pre-Migration Backup Prompt (export JSON)
    ↓
Double Confirmation ("MIGRATE" text input)
    ↓
Migration Process (JSON → SQLite)
    ↓
Now Using SQLite Database
    ↓
Configuration Page Adapts:
  - Backup: Download DB file OR Export JSON
  - Restore: Upload DB file OR Upload JSON
```

## Current State Analysis

### Existing Import/Export Architecture

#### **FileSystem (JSON) Provider**
- **Per-Component Export/Import**: Each configuration section (Components, Delimiters, Environments, etc.) has individual export/import functionality
- **Global Configuration Export/Import**: Single JSON file containing all 13 entity types plus appsettings
- **File Structure**: 17+ individual JSON files in `settings/` folder
- **Configuration Page**: Separate import/export controls for each component section
- **Backup Method**: Export individual component JSON or full configuration JSON

#### **SQLite Provider**
- **Single Database File**: All configuration stored in `azurenamingtool.db` (~132KB)
- **Current Export/Import**: Uses same `ImportExportService` as FileSystem
  - `ExportConfigAsync()`: Queries all 13 entity types via services, returns `ConfigurationData` JSON
  - `PostConfigAsync()`: Imports `ConfigurationData` JSON, writes via entity services
- **Entity Services**: Abstract storage via `IStorageProvider` interface
- **Current Issue**: Configuration page shows same per-component import/export UI, which doesn't make sense for a single database

### Key Insight
✅ **Import ALREADY works with SQLite** - The `ImportExportService.PostConfigAsync()` method uses entity services (like `ResourceTypeService`, `ResourceComponentService`, etc.) which depend on `IStorageProvider`. This abstraction means imports work with **both** FileSystem and SQLite providers.

---

## Problem Statement

### User Experience Issues

1. **Inappropriate UI for SQLite**
   - Configuration page shows individual component import/export sections
   - For SQLite, users can't import/export individual components (it's a single database)
   - Per-component export/import makes sense for JSON files, not for SQLite
   - Creates confusion about what can actually be backed up/restored

2. **Mixed Backup Strategies**
   - JSON files: Can export individual components OR full configuration
   - SQLite: Should only export/import full configuration OR backup database file
   - Current UI doesn't differentiate between storage providers

3. **Database File Backup Not Obvious**
   - Users on SQLite could simply backup `azurenamingtool.db` file directly
   - This is simpler than JSON export/import for full backups
   - Not currently documented or presented as an option

4. **Migration Rollback Confusion**
   - Original plan had rollback from SQLite → JSON
   - New direction: One-way migration only
   - Need to ensure users understand backup options BEFORE migrating

---

## Proposed Solution

### Strategy: Unified Backup/Restore with Dual Import Support

#### **Core Principle**
Users upgrade from JSON (FileSystem) to SQLite through a one-way migration. After migration, they can restore from EITHER SQLite backups OR JSON backups, providing maximum flexibility.

### User Journey

#### **Phase 1: Pre-Migration (Using FileSystem/JSON)**
- ✅ Existing users have persistent storage with JSON files
- ✅ Configuration page shows per-component import/export (current behavior)
- ✅ Global configuration export/import available
- ✅ Users can backup individual components or full configuration as JSON

#### **Phase 2: Migration to SQLite**
- ✅ User sees prominent "Migrate to SQLite" option in Admin page
- ✅ Pre-migration backup prompt strongly encourages backup
- ✅ User can export JSON configuration before migrating
- ✅ Migration automatically transfers all JSON data to SQLite database
- ✅ Original JSON files preserved in storage (not deleted)
- ✅ One-way migration (no automatic rollback)

#### **Phase 3: Post-Migration (Using SQLite)**
- ✅ Configuration page adapts to SQLite mode
- ✅ Per-component import/export hidden (single database model)
- ✅ **Backup**: Users can download SQLite database file
- ✅ **Restore**: Users can import from TWO sources:
  1. **SQLite Database File** (recommended for routine backups)
  2. **JSON Configuration File** (for disaster recovery, migration from another system)

### Backup Strategies

#### **When Using FileSystem (JSON)**
- Individual component JSON files in persistent storage
- Global configuration export as single JSON file
- Manual file system backup of `settings/` folder

#### **When Using SQLite**
- **Primary Backup Method**: Download SQLite database file
  - Fast, simple, complete backup
  - Preserves exact database state
  - Recommended for routine backups
  
- **Secondary Backup Method**: Export as JSON
  - Portable across systems
  - Human-readable format
  - Compatible with pre-migration backups
  - Useful for disaster recovery scenarios

### Restore Capabilities

#### **Scenario 1: Restore from SQLite Backup**
- User has backed up their SQLite database file
- Upload backup database file
- System replaces current database with backup
- Fast, complete restoration
- **Implementation**: Replace `azurenamingtool.db` file or import via UI

#### **Scenario 2: Restore from JSON Backup**
- User has JSON configuration backup (pre-migration or exported post-migration)
- Upload JSON configuration file
- System imports JSON data into SQLite database using existing `ImportExportService`
- Overwrites current SQLite data with JSON data
- **Implementation**: Already supported by `PostConfigAsync` method (works with both providers)

---

## Implementation Plan

### Phase 1: Pre-Migration Improvements

#### 1.1 Add Storage Provider Detection to Configuration Page

**Files to Modify:**
- `src/Components/Pages/Configuration.razor`

**Changes:**
```csharp
@code {
    private string currentStorageProvider = "";
    private bool isUsingSQLite => currentStorageProvider == "SQLite";
    private bool isUsingFileSystem => currentStorageProvider == "FileSystem";
    
    protected override async Task OnInitializedAsync()
    {
        // ... existing initialization ...
        currentStorageProvider = ConfigurationHelper.GetAppSetting("StorageProvider") ?? "FileSystem";
    }
}
```

#### 1.2 Create Conditional UI Rendering

**Approach:**
- Wrap existing per-component import/export sections in `@if (isUsingFileSystem)` blocks
- Create new SQLite-specific backup/restore section
- Keep global configuration section visible for both, but adapt messaging

**Example Structure:**
```razor
@if (isUsingFileSystem)
{
    <!-- Existing per-component sections: Components, Delimiters, Environments, etc. -->
    <div class="card">
        <h5>Components</h5>
        <div>Export the current Components Configuration</div>
        <button>Export</button>
        <div>Import Components Configuration</div>
        <textarea></textarea>
        <button>Import</button>
    </div>
    <!-- ... repeat for all 13 component types ... -->
}

@if (isUsingSQLite)
{
    <!-- New SQLite-specific backup section -->
    <div class="card">
        <h5>Backup & Restore</h5>
        <p>You are using SQLite storage. Two backup options are available:</p>
        
        <div class="card">
            <!-- This old UI structure was replaced with new detailed backup/restore UI above -->
    
    <div class="card">
        <h6>Import Global Configuration</h6>
        <p>Import a full configuration JSON file. This will overwrite all existing settings.</p>
        <textarea></textarea>
        <button>Import</button>
    </div>
</div>
```

#### 1.3 Add Backup and Restore Functionality

**New Methods in Configuration.razor:**

```csharp
// BACKUP METHODS

private async Task DownloadDatabaseFile()
{
    var confirm = await ModalHelper.ShowConfirmationModal(
        Modal!, "Download Database Backup",
        "<div class=\"my-4\">This will download the entire SQLite database file.</div>" +
        "<div class=\"my-4\">Save this file in a secure location as your backup.</div>" +
        "<div class=\"my-4\">You can restore from this file later using the Restore section.</div>",
        "bg-info", theme
    );
    
    if (!confirm) return;
    
    try 
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "azurenamingtool.db");
        
        if (File.Exists(dbPath))
        {
            var bytes = await File.ReadAllBytesAsync(dbPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var filename = $"azurenamingtool-backup-{timestamp}.db";
            
            await BlazorDownloadFileService!.DownloadFile(filename, bytes, "application/x-sqlite3");
            
            await AdminLogService!.PostItemAsync(new AdminLogMessage 
            { 
                Title = "Database Backup Downloaded", 
                Message = $"Database file downloaded: {filename} ({bytes.Length / 1024} KB)" 
            });
            
            toastService.ShowSuccess($"Database backup downloaded: {filename}");
        }
        else
        {
            toastService.ShowError("Database file not found.");
        }
    }
    catch (Exception ex)
    {
        await AdminLogService!.PostItemAsync(new AdminLogMessage 
        { 
            Title = "Database Download Error", 
            Message = ex.Message 
        });
        toastService.ShowError("Failed to download database file.");
    }
}

private async Task ExportConfigurationAsJson()
{
    try 
    {
        var result = await ImportExportService!.ExportConfigAsync(includeAdmin);
        
        if (result.Success && result.ResponseObject != null)
        {
            var json = JsonSerializer.Serialize(result.ResponseObject, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var filename = $"azurenamingtool-config-{timestamp}.json";
            
            await BlazorDownloadFileService!.DownloadFile(filename, bytes, "application/json");
            
            await AdminLogService!.PostItemAsync(new AdminLogMessage 
            { 
                Title = "Configuration Exported", 
                Message = $"Configuration exported as JSON: {filename}" 
            });
            
            toastService.ShowSuccess($"Configuration exported: {filename}");
        }
        else
        {
            toastService.ShowError("Failed to export configuration.");
        }
    }
    catch (Exception ex)
    {
        await AdminLogService!.PostItemAsync(new AdminLogMessage 
        { 
            Title = "Export Error", 
            Message = ex.Message 
        });
        toastService.ShowError("Failed to export configuration.");
    }
}

// RESTORE METHODS

private IBrowserFile? sqliteBackupFile;
private IBrowserFile? jsonBackupFile;

private void HandleSQLiteDatabaseUpload(InputFileChangeEventArgs e)
{
    sqliteBackupFile = e.File;
}

private void HandleJsonConfigUpload(InputFileChangeEventArgs e)
{
    jsonBackupFile = e.File;
}

private async Task RestoreFromSQLiteBackup()
{
    if (sqliteBackupFile == null)
    {
        toastService.ShowError("Please select a database file first.");
        return;
    }
    
    var confirm = await ModalHelper.ShowConfirmationModal(
        Modal!, "RESTORE FROM DATABASE BACKUP",
        "<div class=\"my-4\"><strong>⚠️ This will OVERWRITE your current database!</strong></div>" +
        "<div class=\"my-4\">Your current configuration will be replaced with the backup database.</div>" +
        "<div class=\"my-4\">Make sure you have backed up your current database if needed.</div>" +
        "<div class=\"my-4\">Are you sure you want to proceed?</div>",
        "bg-danger", theme
    );
    
    if (!confirm) return;
    
    try 
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "azurenamingtool.db");
        
        // Read uploaded file
        using var stream = sqliteBackupFile.OpenReadStream(maxAllowedSize: 10485760); // 10MB max
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        
        // Validate it's a SQLite database (basic check)
        if (bytes.Length < 16 || 
            System.Text.Encoding.ASCII.GetString(bytes, 0, 16) != "SQLite format 3\0")
        {
            toastService.ShowError("Invalid SQLite database file.");
            return;
        }
        
        // Replace current database file
        await File.WriteAllBytesAsync(dbPath, bytes);
        
        await AdminLogService!.PostItemAsync(new AdminLogMessage 
        { 
            Title = "Database Restored", 
            Message = $"Database restored from backup: {sqliteBackupFile.Name} ({bytes.Length / 1024} KB)" 
        });
        
        toastService.ShowSuccess("Database restored successfully! Please restart the application.");
        sqliteBackupFile = null;
    }
    catch (Exception ex)
    {
        await AdminLogService!.PostItemAsync(new AdminLogMessage 
        { 
            Title = "Database Restore Error", 
            Message = ex.Message 
        });
        toastService.ShowError($"Failed to restore database: {ex.Message}");
    }
}

private async Task RestoreFromJsonBackup()
{
    if (jsonBackupFile == null)
    {
        toastService.ShowError("Please select a JSON configuration file first.");
        return;
    }
    
    var confirm = await ModalHelper.ShowConfirmationModal(
        Modal!, "RESTORE FROM JSON BACKUP",
        "<div class=\"my-4\"><strong>⚠️ This will OVERWRITE your current configuration!</strong></div>" +
        "<div class=\"my-4\">Your current SQLite database will be updated with the JSON configuration data.</div>" +
        "<div class=\"my-4\">This works with both pre-migration JSON files and post-migration JSON exports.</div>" +
        "<div class=\"my-4\">Are you sure you want to proceed?</div>",
        "bg-danger", theme
    );
    
    if (!confirm) return;
    
    try 
    {
        // Read uploaded JSON file
        using var stream = jsonBackupFile.OpenReadStream(maxAllowedSize: 10485760); // 10MB max
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        
        // Deserialize to ConfigurationData
        var configData = JsonSerializer.Deserialize<ConfigurationData>(json);
        
        if (configData == null)
        {
            toastService.ShowError("Invalid JSON configuration file.");
            return;
        }
        
        // Import using existing ImportExportService (works with SQLite!)
        var result = await ImportExportService!.PostConfigAsync(configData);
        
        if (result.Success)
        {
            await AdminLogService!.PostItemAsync(new AdminLogMessage 
            { 
                Title = "Configuration Restored from JSON", 
                Message = $"Configuration imported from JSON backup: {jsonBackupFile.Name}" 
            });
            
            toastService.ShowSuccess("Configuration restored from JSON successfully!");
            jsonBackupFile = null;
        }
        else
        {
            toastService.ShowError($"Failed to import configuration: {result.ResponseMessage}");
        }
    }
    catch (Exception ex)
    {
        await AdminLogService!.PostItemAsync(new AdminLogMessage 
        { 
            Title = "JSON Restore Error", 
            Message = ex.Message 
        });
        toastService.ShowError($"Failed to restore from JSON: {ex.Message}");
    }
}

private string GetDatabaseSize()
{
    try
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "azurenamingtool.db");
        if (File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            return $"{fileInfo.Length / 1024} KB";
        }
    }
    catch { }
    return "Unknown";
}
```

---

### Phase 2: One-Way Migration Implementation

#### 2.1 Pre-Migration Backup Encouragement

**Admin.razor - Migration Section Updates:**

```razor
@if (currentStorageProvider == "FileSystem")
{
    <div class="card mb-3">
        <h5>Migrate to SQLite Database</h5>
        
        <div class="alert alert-info">
            <h6>⚠️ Important: One-Way Migration</h6>
            <ul>
                <li>Migration to SQLite is <strong>permanent and cannot be reversed</strong></li>
                <li>There is <strong>no rollback</strong> option after migration</li>
                <li>SQLite provides better performance and reliability</li>
                <li><strong>You should export your configuration before migrating</strong></li>
            </ul>
        </div>
        
        <div class="card mb-3 bg-light">
            <div class="card-body">
                <h6>Step 1: Backup Your Configuration (Recommended)</h6>
                <p>Export your current configuration before migrating. You can import this later if needed.</p>
                <button class="btn btn-primary" @onclick="ExportBeforeMigration">
                    <span class="oi oi-data-transfer-download"></span> Export Configuration Now
                </button>
                <div class="form-check mt-2">
                    <input class="form-check-input" type="checkbox" @bind="hasExportedBeforeMigration" id="exportCheck" />
                    <label class="form-check-label" for="exportCheck">
                        I have already exported my configuration
                    </label>
                </div>
            </div>
        </div>
        
        <div class="card mb-3">
            <div class="card-body">
                <h6>Step 2: Migrate to SQLite</h6>
                <p>Proceed with migration after backing up your configuration.</p>
                <button class="btn btn-warning" @onclick="InitiateMigration" 
                        disabled="@(!hasExportedBeforeMigration)">
                    <span class="oi oi-transfer"></span> Migrate to SQLite
                </button>
                @if (!hasExportedBeforeMigration)
                {
                    <p class="text-muted mt-2">
                        <small>You must export or confirm backup before migrating.</small>
                    </p>
                }
            </div>
        </div>
    </div>
}
else if (currentStorageProvider == "SQLite")
{
    <div class="card mb-3">
        <h5>Storage Provider: SQLite Database</h5>
        <div class="alert alert-success">
            <p>✅ You are using SQLite database storage.</p>
            <p>Visit the <a href="/configuration">Configuration page</a> to backup your database.</p>
        </div>
    </div>
}
```

#### 2.2 Double Confirmation Modal Flow

**Modal Sequence:**

**First Modal** (triggered by "Migrate to SQLite" button):
```csharp
private async Task InitiateMigration()
{
    // First confirmation - Explain permanence
    var firstConfirm = await ModalHelper.ShowConfirmationModal(
        Modal!, 
        "ATTENTION: Permanent Migration",
        "<div class=\"my-4\"><strong>⚠️ This migration is ONE-WAY and CANNOT be reversed</strong></div>" +
        "<div class=\"my-4\">After migration:</div>" +
        "<ul>" +
        "<li>You will be using SQLite database storage permanently</li>" +
        "<li>There is NO rollback to JSON files</li>" +
        "<li>Your configuration will be migrated to an embedded database</li>" +
        "<li>You can still export/import JSON configurations if needed</li>" +
        "</ul>" +
        "<div class=\"my-4\">Make sure you have backed up your configuration!</div>" +
        "<div class=\"my-4\">Do you want to continue?</div>",
        "bg-warning",
        theme
    );
    
    if (!firstConfirm) return;
    
    // Second confirmation - Final warning with text input
    var parameters = new ModalParameters();
    parameters.Add("Title", "FINAL CONFIRMATION");
    parameters.Add("Message", 
        "<div class=\"my-4\"><strong>⚠️ FINAL WARNING</strong></div>" +
        "<div class=\"my-4\">You are about to permanently migrate to SQLite.</div>" +
        "<div class=\"my-4\">This action is <strong>IRREVERSIBLE</strong>.</div>" +
        "<div class=\"my-4\">Type <code>MIGRATE</code> below to confirm:</div>");
    parameters.Add("Theme", theme);
    parameters.Add("RequiredText", "MIGRATE");
    parameters.Add("HeaderStyle", "bg-danger");
    
    var options = new ModalOptions { HideCloseButton = true };
    var modal = Modal!.Show<TextConfirmationModal>("", parameters, options);
    var result = await modal.Result;
    
    if (result.Cancelled) return;
    
    // Proceed with migration
    await PerformMigration();
}

private async Task PerformMigration()
{
    migrating = true;
    StateHasChanged();
    
    try
    {
        var result = await MigrationService!.MigrateToSQLiteAsync();
        
        if (result)
        {
            await LogHelper.LogAdminMessage(
                AdminLogService!, 
                "Migration to SQLite completed successfully"
            );
            
            toastService.ShowSuccess(
                "Migration completed successfully! Please restart the application.",
                "Migration Complete"
            );
        }
        else
        {
            await LogHelper.LogAdminMessage(
                AdminLogService!, 
                "Migration to SQLite failed"
            );
            
            toastService.ShowError(
                "Migration failed. Check admin logs for details.",
                "Migration Failed"
            );
        }
    }
    catch (Exception ex)
    {
        await LogHelper.LogAdminMessage(
            AdminLogService!, 
            "Migration error", 
            ex.Message
        );
        
        toastService.ShowError($"Migration error: {ex.Message}", "Error");
    }
    finally
    {
        migrating = false;
        StateHasChanged();
    }
}
```

#### 2.3 Remove Rollback Functionality

**Admin.razor Changes:**
- ❌ Delete `InitiateRollback()` method (lines ~1151-1220)
- ❌ Delete rollback UI section (lines ~419-456)
- ❌ Remove `rollbackPath` variable and related state
- ✅ Update migration status display to show "Migration Date" only, no backup path
- ✅ Remove "Rollback to JSON" card entirely

**StorageMigrationService.cs:**
- ⚠️ Decision needed: Keep `RollbackMigrationAsync()` method for emergency admin use, or remove completely?
- ✅ Recommendation: **Keep method but remove from UI** - useful for disaster recovery via direct service call

---

### Phase 3: New Modal Component

#### 3.1 Create TextConfirmationModal

**New File:** `src/Components/Modals/TextConfirmationModal.razor`

```razor
@using AzureNamingTool.Models

<div class="modal-body @theme.ThemeStyle">
    <div class="p-3">
        @((MarkupString)Message)
        
        <div class="form-group mt-3">
            <input type="text" 
                   class="form-control @theme.ThemeStyle" 
                   @bind="userInput" 
                   @bind:event="oninput"
                   placeholder="@RequiredText"
                   autofocus />
        </div>
        
        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="alert alert-danger mt-2">@errorMessage</div>
        }
    </div>
</div>

<div class="modal-footer @theme.ThemeStyle">
    <button type="button" 
            class="btn btn-danger" 
            @onclick="Confirm"
            disabled="@(!IsValid)">
        Confirm
    </button>
    <button type="button" 
            class="btn btn-secondary" 
            @onclick="Cancel">
        Cancel
    </button>
</div>

@code {
    [CascadingParameter]
    BlazoredModalInstance? BlazoredModal { get; set; }
    
    [Parameter]
    public string Title { get; set; } = "Confirmation";
    
    [Parameter]
    public string Message { get; set; } = "";
    
    [Parameter]
    public string RequiredText { get; set; } = "";
    
    [Parameter]
    public string HeaderStyle { get; set; } = "bg-warning";
    
    [Parameter]
    public SiteConfiguration? theme { get; set; }
    
    private string userInput = "";
    private string errorMessage = "";
    
    private bool IsValid => userInput.Trim().Equals(RequiredText, StringComparison.Ordinal);
    
    private async Task Confirm()
    {
        if (IsValid)
        {
            await BlazoredModal!.CloseAsync(ModalResult.Ok(true));
        }
        else
        {
            errorMessage = $"You must type '{RequiredText}' exactly to confirm.";
        }
    }
    
    private async Task Cancel()
    {
        await BlazoredModal!.CancelAsync();
    }
}
```

---

### Phase 4: Documentation Updates

#### 4.1 Migration Guide

**New Section in README or separate MIGRATION_GUIDE.md:**

```markdown
# SQLite Migration Guide

## Overview
Azure Naming Tool supports two storage providers:
- **FileSystem**: Individual JSON files (legacy, default for existing installations)
- **SQLite**: Embedded database (recommended for new installations and migrations)

## Important: One-Way Migration
⚠️ **Migration to SQLite is permanent and cannot be reversed.**

There is no automatic rollback from SQLite to JSON files. If you need to return to JSON file storage, you must:
1. Export your configuration as JSON
2. Reinstall the application
3. Import the JSON configuration

## Before Migrating

### Step 1: Export Your Configuration
1. Navigate to **Configuration** page
2. Scroll to **Global Configuration** section
3. Click **Export** to download your full configuration as JSON
4. Save the file in a secure location (e.g., `azurenamingtool-backup-{timestamp}.json`)
5. Optionally check "Include Security/Identity Provider Settings" if you're an admin

### Step 2: Verify Your Backup
1. Open the exported JSON file in a text editor
2. Verify it contains your configuration data
3. Ensure the file is not corrupted

## Migration Process

### Step 1: Initiate Migration
1. Navigate to **Admin** page
2. Scroll to **Storage Provider** section
3. Click **Export Configuration Now** (or check "I have already exported")
4. Click **Migrate to SQLite** button

### Step 2: Confirm Migration
1. Read the first confirmation dialog carefully
2. Click **OK** to proceed
3. Read the final confirmation dialog
4. Type **MIGRATE** in the text field (case-sensitive)
5. Click **Confirm**

### Step 3: Wait for Completion
- Migration typically takes 10-30 seconds
- All 13 configuration entity types will be migrated
- Application will log progress to Admin Log
- Do not close the browser during migration

### Step 4: Restart Application
- After successful migration, restart the application
- Verify all configuration data is present
- Check Admin Log for any errors

## After Migration

### Backup Options with SQLite

You now have **two backup methods** available:

#### Option 1: Export Configuration (JSON)
**Recommended for:** Portability, disaster recovery, moving to different server
- Navigate to **Configuration** page
- Click **Export Full Configuration**
- Saves as JSON file (~500KB - 2MB depending on data)
- Can be imported on any system

#### Option 2: Database File Backup
**Recommended for:** Quick backups, exact replica, scheduled backups
- Navigate to **Configuration** page
- Click **Download Database File**
- Saves as `azurenamingtool-backup-{timestamp}.db`
- Smaller file size (~132KB for typical configuration)
- Can be restored by replacing `settings/azurenamingtool.db`

### Restoring from Backup

#### Restore from JSON Export
1. Navigate to **Configuration** page
2. Scroll to **Restore Configuration** section
3. Upload your JSON backup file
4. Click **Import Configuration**
5. Confirm the import (will overwrite existing data)
6. Restart application

#### Restore from Database File
1. Stop the application
2. Navigate to `settings/` folder
3. Replace `azurenamingtool.db` with your backup file
4. Restart the application

## Troubleshooting

### Migration Failed
- Check **Admin Log** for error details
- Verify JSON files exist in `settings/` folder
- Ensure database is not locked by another process
- Try restarting application and retrying migration

### Cannot Access Application After Migration
1. Stop the application
2. Delete `settings/azurenamingtool.db` if it exists
3. Verify JSON files are present in `settings/` folder
4. Edit `settings/appsettings.json`:
   - Change `"StorageProvider": "SQLite"` to `"StorageProvider": "FileSystem"`
5. Restart application
6. Application should load with JSON files

### Data Missing After Migration
- Check Admin Log for migration errors
- Verify source JSON files contained data
- If you have a pre-migration JSON export:
  1. Navigate to Configuration page
  2. Import the JSON export
  3. All data will be restored

## FAQ

### Can I switch back to JSON files?
No. Migration is one-way. However, you can always export your configuration as JSON and import it into a fresh FileSystem installation.

### Will my JSON files be deleted?
No. JSON files remain in the `settings/` folder after migration. They are not used by SQLite but are kept as a safety backup.

### Can I import individual components after migration?
No. SQLite uses a single database file, so individual component import/export is not available. You can restore your full configuration using either a SQLite database backup or a JSON configuration backup.

### What if I want to migrate back?
To return to JSON file storage:
1. Export your configuration as JSON (while using SQLite)
2. Install a fresh copy of Azure Naming Tool
3. Do not migrate to SQLite
4. Import your JSON configuration
5. You will now be using FileSystem provider

### Can I use my old JSON backup files after migrating to SQLite?
Yes! The Scenario 2 restore option allows you to upload your pre-migration JSON files. The system will import that JSON data into your SQLite database, preserving all your configuration.

### How do I schedule automatic backups?
For database file backups, create a scheduled task to copy `settings/azurenamingtool.db` to a backup location:
```powershell
# Windows Task Scheduler - PowerShell script
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
Copy-Item "C:\path\to\app\settings\azurenamingtool.db" `
  -Destination "C:\backups\azurenamingtool-backup-$timestamp.db"
```

For JSON backups via API endpoint:
```bash
curl -X GET "https://your-server/api/ImportExport/ExportConfiguration?includeAdmin=false" \
  -H "APIKey: your-api-key" \
  -o "backup-$(date +%Y%m%d).json"
```

### What's the difference between the two restore scenarios?
- **Scenario 1 (SQLite)**: Replaces your entire database file. Fastest method, exact replica. Requires application restart.
- **Scenario 2 (JSON)**: Imports JSON data into your existing database. Works with pre-migration or post-migration JSON files. No restart needed.

### Which backup method should I use?
- **Routine backups**: Use SQLite database download (Scenario 1). It's faster and smaller.
- **Disaster recovery**: Use JSON export (Scenario 2). It's more portable and compatible across systems.
- **Best practice**: Use both! Download database regularly, export JSON periodically for safety.
```

#### 4.2 Update MODERNIZATION_PLAN.md

**Add new section:**

```markdown
## Phase 6 & 7 Updates: SQLite Migration

### Storage Provider Decision
- ✅ SQLite migration implemented as **one-way** (Phase 6)
- ❌ Rollback removed (Phase 7.1)
- ✅ Configuration page UI adapts to storage provider (Phase 7.1)

### Backup Strategy
#### FileSystem (JSON)
- Per-component export/import available
- Global configuration export/import available
- Manual file backup of `settings/` folder

#### SQLite
- Database file download for quick backups
- JSON export for portable backups
- JSON import for restore (works with SQLite)
- No per-component import/export (single database)

### Migration Process
1. Pre-migration backup encouragement (required checkbox)
2. Double confirmation with text input ("MIGRATE")
3. Migration completes (10-30 seconds)
4. Application restart required
5. No rollback option available

### Implementation Status
- ✅ Phase 6: SQLite storage provider and migration service
- ✅ Phase 7.1: One-way migration with pre-flight backup
- ⏳ Phase 7.1: Configuration page UI adaptation (in progress)
- ⏳ Phase 7.1: Database file download feature (in progress)
- ⏳ Phase 7.1: Documentation updates (in progress)
```

---

## Testing Plan

### Test Case 1: FileSystem Configuration Page
**When:** Storage provider is FileSystem
- ✅ Per-component export/import sections visible
- ✅ Each component (Components, Delimiters, etc.) has export/import
- ✅ Global configuration section visible
- ✅ Can export individual component JSON
- ✅ Can import individual component JSON
- ✅ Can export full global configuration
- ✅ Can import full global configuration

### Test Case 2: SQLite Configuration Page
**When:** Storage provider is SQLite
- ✅ Per-component export/import sections HIDDEN
- ✅ New "Backup & Restore" section visible
- ✅ "Export Full Configuration" button works
- ✅ "Download Database File" button works
- ✅ "Import Configuration" file upload works
- ✅ Database file size shown correctly
- ✅ JSON import works and overwrites existing data

### Test Case 3: Pre-Migration Backup
**When:** Initiating migration from FileSystem
- ✅ Export button downloads JSON configuration
- ✅ Checkbox "I have already exported" available
- ✅ Migration button disabled until export or checkbox
- ✅ Exported JSON file is valid and contains all data

### Test Case 4: Migration Double Confirmation
**When:** Migrating to SQLite
- ✅ First modal explains one-way migration
- ✅ First modal has clear warning about no rollback
- ✅ Second modal requires typing "MIGRATE"
- ✅ Second modal won't proceed with wrong text
- ✅ Can cancel at any point
- ✅ Migration only proceeds after both confirmations

### Test Case 5: Post-Migration State
**When:** After successful migration
- ✅ Storage provider is SQLite
- ✅ Configuration page shows SQLite UI
- ✅ Rollback section NOT visible in Admin page
- ✅ Migration date shown (no backup path)
- ✅ Database file exists in settings folder
- ✅ JSON files still exist (not deleted)
- ✅ All configuration data migrated correctly

### Test Case 6: Restore After Migration - Scenario 1 (SQLite)
**When:** Using SQLite and restoring from database backup
- ✅ Upload `.db` file via file input works
- ✅ Database file validation checks for SQLite signature
- ✅ Current database replaced with backup
- ✅ Restore button disabled until file selected
- ✅ Confirmation modal shows clear warning
- ✅ Admin log records restore action
- ✅ Success message instructs to restart application
- ✅ After restart, all configuration matches backup

### Test Case 7: Restore After Migration - Scenario 2 (JSON)
**When:** Using SQLite and restoring from JSON backup
- ✅ Upload `.json` file via file input works
- ✅ Can restore from pre-migration JSON files
- ✅ Can restore from post-migration JSON exports
- ✅ JSON imported into SQLite database correctly
- ✅ Restore button disabled until file selected
- ✅ Confirmation modal shows clear warning
- ✅ Admin log records import action
- ✅ All entity types imported correctly
- ✅ Configuration updated without restart (cache cleared)

### Test Case 8: Database File Download & Upload
**When:** Backup and restore with database files
- ✅ Download creates timestamped `.db` file
- ✅ Downloaded file is valid SQLite database
- ✅ File size matches displayed size
- ✅ Upload accepts `.db` files only
- ✅ Validates SQLite signature before restore
- ✅ Admin log records both download and restore actions
- ✅ Round-trip test: download → restore → verify data intact

### Test Case 9: JSON Export & Import Round Trip
**When:** Backup and restore with JSON files
- ✅ Export creates timestamped `.json` file
- ✅ Exported JSON is valid and well-formed
- ✅ Upload accepts `.json` files only
- ✅ Can import same JSON that was just exported
- ✅ Data matches after round trip
- ✅ Admin log records both export and import actions

### Test Case 10: Error Handling
**When:** Various error scenarios
- ✅ Migration fails gracefully with error message
- ✅ Database locked error handled appropriately
- ✅ Import with invalid JSON shows error
- ✅ Download database when file missing shows error
- ✅ All errors logged to Admin Log

---

## Implementation Checklist

### Phase 1: Configuration Page Adaptation
- [ ] Add storage provider detection
- [ ] Create conditional rendering for FileSystem sections
- [ ] Create new SQLite backup/restore section with dual restore support
- [ ] Implement database file download method (backup)
- [ ] Implement JSON export method (alternative backup)
- [ ] Implement SQLite database upload/restore method (Scenario 1)
- [ ] Implement JSON configuration upload/import method (Scenario 2)
- [ ] Add database file size display
- [ ] Add file upload validation (file type, size, format)
- [ ] Test UI switches correctly based on provider
- [ ] Test per-component import/export only visible for FileSystem
- [ ] Test both restore scenarios work correctly

### Phase 2: Admin Page Migration Updates
- [ ] Create TextConfirmationModal component
- [ ] Add pre-migration backup section
- [ ] Implement export before migration
- [ ] Add "I have already exported" checkbox
- [ ] Disable migration button until backup confirmed
- [ ] Update InitiateMigration() with double confirmation
- [ ] Add text input validation ("MIGRATE")
- [ ] Remove InitiateRollback() method completely
- [ ] Remove rollback UI section
- [ ] Update migration status display
- [ ] Remove rollback-related state variables
- [ ] Test full migration flow end-to-end

### Phase 3: Documentation
- [ ] Create MIGRATION_GUIDE.md with full instructions
- [ ] Update MODERNIZATION_PLAN.md with Phase 6/7 status
- [ ] Update README.md with migration overview
- [ ] Document two backup methods for SQLite
- [ ] Document disaster recovery process
- [ ] Add FAQ section for common questions
- [ ] Create troubleshooting guide

### Phase 4: Testing
- [ ] Test all test cases listed above
- [ ] Verify Configuration page for FileSystem
- [ ] Verify Configuration page for SQLite
- [ ] Test pre-migration backup process
- [ ] Test migration double confirmation
- [ ] Test post-migration state
- [ ] Test Scenario 1: SQLite database backup → restore
- [ ] Test Scenario 2: JSON export → restore (post-migration)
- [ ] Test Scenario 2: Pre-migration JSON files → restore (into SQLite)
- [ ] Test database file download/upload validation
- [ ] Test JSON file upload validation
- [ ] Test round-trip: backup → restore → verify data
- [ ] Test error scenarios (invalid files, corrupted data, etc.)
- [ ] Test with different data volumes
- [ ] Test file size limits (10MB max)
- [ ] Test on Windows/Linux/Mac (if applicable)
- [ ] Test that original JSON files remain after migration (not deleted)

### Phase 5: Code Review & Polish
- [ ] Review all UI messages for clarity
- [ ] Ensure consistent terminology (FileSystem vs JSON, SQLite vs Database)
- [ ] Check accessibility (ARIA labels, keyboard navigation)
- [ ] Verify theme styles applied correctly
- [ ] Check mobile responsiveness
- [ ] Review error messages for helpfulness
- [ ] Ensure admin log messages are clear
- [ ] Code review for security issues

---

## File Changes Summary

### Files to Create
1. `docs/v5.0.0/development/MIGRATIONGUIDANCE_PLAN.md` (this file)
2. `docs/v5.0.0/V5.0.0_MIGRATION_GUIDE.md` (user-facing documentation)
3. `src/Components/Modals/TextConfirmationModal.razor` (new modal component)

### Files to Modify
1. `src/Components/Pages/Configuration.razor`
   - Add storage provider detection
   - Add conditional rendering for FileSystem vs SQLite UI
   - Add database file download method
   - Add SQLite backup/restore section

2. `src/Components/Pages/Admin.razor`
   - Remove rollback functionality (method + UI)
   - Add pre-migration backup section
   - Update InitiateMigration() with double confirmation
   - Update migration status display

3. `docs/v5.0.0/development/MODERNIZATION_PLAN.md`
   - Update Phase 6 & 7 status
   - Document storage provider decision
   - Document backup strategy

4. `README.md`
   - Add migration overview
   - Link to detailed migration guide

### Files to Consider
1. `src/Services/StorageMigrationService.cs`
   - Decision: Keep or remove `RollbackMigrationAsync()` method?
   - Recommendation: Keep but don't expose in UI (emergency use only)

---

## Success Criteria

### User Experience
- ✅ Users clearly understand migration is one-way
- ✅ Users are encouraged to backup before migration
- ✅ Double confirmation prevents accidental migration
- ✅ Configuration page UI makes sense for current storage provider
- ✅ Backup process is simple and clearly documented
- ✅ Import/restore process is straightforward

### Technical
- ✅ No per-component import/export visible for SQLite
- ✅ Database file download works correctly
- ✅ JSON import works with SQLite provider
- ✅ Migration cannot proceed without backup confirmation
- ✅ Rollback functionality completely removed from UI
- ✅ All operations logged to Admin Log
- ✅ Error handling is robust

### Documentation
- ✅ Migration process fully documented
- ✅ Backup methods clearly explained
- ✅ Restore process documented for both backup types
- ✅ Troubleshooting guide available
- ✅ FAQ answers common questions
- ✅ Disaster recovery process documented

---

## Future Considerations

### Potential Enhancements (Post-Implementation)

1. **Scheduled Backup Automation**
   - Add scheduled export to external storage (Azure Blob, AWS S3, etc.)
   - Configurable backup frequency
   - Retention policy management

2. **Backup Validation**
   - Verify exported JSON is valid before allowing migration
   - Test import in temporary database before applying
   - Checksum validation for database file backups

3. **Migration Metrics**
   - Track migration duration
   - Record database size before/after
   - Log entity counts for verification

4. **Rollback Emergency Procedure**
   - Document manual rollback process for support team
   - Create admin-only API endpoint for emergency rollback
   - Require admin authentication + text confirmation

5. **Multi-Database Support**
   - PostgreSQL provider for enterprise deployments
   - Azure SQL provider for cloud deployments
   - Migration between different database types

---

## Risk Assessment

### High Risk
- ⚠️ **Users migrate without backup**: Mitigated by required export step
- ⚠️ **Data loss during migration**: Mitigated by keeping JSON files + backup creation
- ⚠️ **Cannot restore after migration**: Mitigated by JSON import compatibility

### Medium Risk
- ⚠️ **Confusion about backup methods**: Mitigated by clear documentation + UI guidance
- ⚠️ **Accidental migration**: Mitigated by double confirmation + text input
- ⚠️ **Database corruption**: Mitigated by JSON export option + file backups

### Low Risk
- ⚠️ **Performance issues with SQLite**: SQLite handles expected data volumes well
- ⚠️ **Compatibility issues**: SQLite is cross-platform and widely supported
- ⚠️ **UI confusion**: Clear labeling and provider-specific UI reduces confusion

---

## Approval & Sign-off

- [ ] Plan reviewed by project maintainer
- [ ] Technical approach approved
- [ ] UI/UX approach approved
- [ ] Documentation approach approved
- [ ] Risk assessment acceptable
- [ ] Ready to begin implementation

---

## Implementation Timeline (Estimated)

- **Phase 1** (Configuration Page Adaptation): 4-6 hours
- **Phase 2** (Admin Page Migration Updates): 6-8 hours
- **Phase 3** (Documentation): 3-4 hours
- **Phase 4** (Testing): 4-6 hours
- **Phase 5** (Code Review & Polish): 2-3 hours

**Total Estimated Time**: 19-27 hours

---

## Notes

- This plan assumes the current `ImportExportService` already works with SQLite (which it does via entity services)
- JSON files will NOT be deleted during migration (kept as safety backup)
- Database file backup is simpler than JSON export but less portable
- Both backup methods should be offered to users for flexibility
- Migration is one-way to simplify maintenance and reduce complexity
- Documentation is critical for user confidence in the migration process

---

**Document Version**: 1.0  
**Created**: 2025-10-16  
**Last Updated**: 2025-10-16  
**Status**: DRAFT - Awaiting Approval

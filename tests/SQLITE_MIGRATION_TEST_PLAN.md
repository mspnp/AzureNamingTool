# SQLite Migration Upgrade Test Plan
**Version:** v4.3.2 → v5.0.0  
**Date:** October 31, 2025  
**Tester:** Bryan Soltis  
**Environment:** Azure App Service (azurenamingtool-dev)

## Objective
Validate the complete upgrade path from v4.3.2 (JSON-based) to v5.0.0 (SQLite-based) including data migration, backup, and restore functionality.

## Test Artifacts
- **v4.3.2 Package:** `C:\Users\brsoltis\Downloads\ANT\v4\AzureNamingTool.zip`
- **v5.0.0 Package:** `c:\Projects\AzureNamingTool-DEV\publish\deploy.zip`
- **Test Site:** azurenamingtool-dev.azurewebsites.net

---

## Test Phases

### Phase 1: Deploy v4.3.2 Baseline
**Objective:** Establish baseline with v4.3.2 running with JSON-based storage

#### Step 1.1: Deploy v4.3.2 to Azure
- [ ] Deploy `C:\Users\brsoltis\Downloads\ANT\v4\AzureNamingTool.zip` to azurenamingtool-dev
- [ ] Wait for deployment to complete (RuntimeSuccessful status)
- [ ] Record deployment ID and timestamp

**Expected Result:** Deployment succeeds

#### Step 1.2: Verify v4.3.2 Site Functionality
- [ ] Navigate to https://azurenamingtool-dev.azurewebsites.net
- [ ] Verify site loads without errors
- [ ] Check version number displays v4.3.2
- [ ] Verify admin login works
- [ ] Navigate to Configuration page
- [ ] Add/Edit at least one configuration item (e.g., Resource Type, Location, Environment)
- [ ] Navigate to Generate page
- [ ] Generate at least 3 test names with different configurations
- [ ] Verify names are saved in Generated Names Log

**Expected Result:** All functionality works, data persists across page refreshes

#### Step 1.3: Export v4.3.2 Configuration
- [ ] Navigate to Configuration → Backup/Restore
- [ ] Click "Export Configuration" button
- [ ] Download and save JSON export file
- [ ] Save as: `v4.3.2-baseline-export.json`
- [ ] Verify JSON file contains all configuration data
- [ ] Record file size and number of configuration items

**Expected Result:** Complete JSON export with all configurations

**Checkpoint:** Save the exported JSON file to a safe location for import testing

---

### Phase 2: Upgrade to v5.0.0
**Objective:** Deploy v5.0.0 and verify automatic migration to SQLite

#### Step 2.1: Deploy v5.0.0 to Azure
- [ ] Deploy `c:\Projects\AzureNamingTool-DEV\publish\deploy.zip` to azurenamingtool-dev
- [ ] Wait for deployment to complete (RuntimeSuccessful status)
- [ ] Record deployment ID and timestamp

**Expected Result:** Deployment succeeds

#### Step 2.2: Verify v5.0.0 Initial Launch
- [ ] Navigate to https://azurenamingtool-dev.azurewebsites.net
- [ ] Verify site loads without errors
- [ ] Check version number displays v5.0.0
- [ ] **CRITICAL:** Check for SQLite migration modal/prompt
- [ ] Do NOT migrate yet - just verify the prompt appears
- [ ] Verify admin login still works
- [ ] Navigate to Configuration page
- [ ] Verify all v4.3.2 data is still accessible (still using JSON files)

**Expected Result:** Site runs on v5.0.0, still using JSON storage, migration prompt appears

---

### Phase 3: Import Configuration (JSON to JSON)
**Objective:** Verify JSON import works before SQLite migration

#### Step 3.1: Import v4.3.2 Configuration
- [ ] Navigate to Configuration → Backup/Restore
- [ ] Click "Import Configuration" button
- [ ] Select the `v4.3.2-baseline-export.json` file
- [ ] Confirm import operation
- [ ] Wait for import to complete
- [ ] Verify success message appears

**Expected Result:** Import succeeds without errors

#### Step 3.2: Verify Imported Data
- [ ] Navigate to each configuration section:
  - [ ] Resource Types
  - [ ] Resource Locations
  - [ ] Resource Environments
  - [ ] Resource Orgs
  - [ ] Resource Units/Depts
  - [ ] Resource Functions
  - [ ] Custom Components
- [ ] Verify all items from export are present
- [ ] Navigate to Generated Names Log
- [ ] Verify historical generated names are present

**Expected Result:** All configuration data successfully imported

---

### Phase 4: Migrate to SQLite
**Objective:** Execute JSON → SQLite migration and validate data integrity

#### Step 4.1: Initiate SQLite Migration
- [ ] Refresh the site (or navigate to home page)
- [ ] Verify SQLite migration modal appears
- [ ] Read the migration prompt carefully
- [ ] Click "Migrate to SQLite" button
- [ ] Wait for migration process to complete
- [ ] Record migration duration
- [ ] Verify success message appears

**Expected Result:** Migration completes successfully

#### Step 4.2: Verify Post-Migration Site Functionality
- [ ] Site should reload automatically
- [ ] Verify site loads without errors
- [ ] Check that migration modal no longer appears
- [ ] Verify admin login works
- [ ] Navigate to Configuration page
- [ ] Verify all configuration items are present

**Expected Result:** Site runs smoothly on SQLite storage

#### Step 4.3: Verify Data Integrity After Migration
- [ ] Check each configuration section for completeness:
  - [ ] Resource Types - count matches pre-migration
  - [ ] Resource Locations - count matches pre-migration
  - [ ] Resource Environments - count matches pre-migration
  - [ ] Resource Orgs - count matches pre-migration
  - [ ] Resource Units/Depts - count matches pre-migration
  - [ ] Resource Functions - count matches pre-migration
  - [ ] Custom Components - count matches pre-migration
- [ ] Navigate to Generated Names Log
- [ ] Verify all historical names migrated
- [ ] Record total count of generated names

**Expected Result:** All data migrated successfully, counts match

#### Step 4.4: Test CRUD Operations on SQLite
- [ ] Add a new Resource Type
- [ ] Edit an existing Resource Location
- [ ] Delete a test item (then re-add it)
- [ ] Navigate to Generate page
- [ ] Generate 3 new names
- [ ] Verify names appear in Generated Names Log
- [ ] Verify all operations persist across page refreshes

**Expected Result:** All CRUD operations work correctly with SQLite

---

### Phase 5: SQLite Backup/Restore Testing
**Objective:** Validate SQLite backup export and import functionality

#### Step 5.1: Export SQLite Backup
- [ ] Navigate to Configuration → Backup/Restore
- [ ] Verify "Export SQLite Backup" button is present
- [ ] Click "Export SQLite Backup" button
- [ ] Download the SQLite database file
- [ ] Save as: `v5.0.0-sqlite-backup.db`
- [ ] Verify file downloads successfully
- [ ] Record file size
- [ ] Verify file is a valid SQLite database (can open with SQLite viewer)

**Expected Result:** SQLite backup exports successfully

#### Step 5.2: Modify Configuration (Create Diff State)
- [ ] Add 2-3 new Resource Types with unique names
- [ ] Add 1-2 new Resource Locations
- [ ] Generate 2-3 new test names
- [ ] Record the new items added (for verification after restore)

**Expected Result:** New items successfully added to current state

#### Step 5.3: Reset Site Configuration
- [ ] Navigate to Configuration → Backup/Restore
- [ ] Locate "Reset Configuration" or similar option
- [ ] Confirm reset operation
- [ ] Wait for reset to complete
- [ ] Verify site reloads

**Expected Result:** Configuration reset successfully

#### Step 5.4: Verify Reset State
- [ ] Navigate to Configuration page
- [ ] Verify configuration is reset to defaults/empty
- [ ] Check Generated Names Log is empty/reset
- [ ] Verify the items added in Step 5.2 are NOT present

**Expected Result:** Site is in clean/reset state

#### Step 5.5: Import SQLite Backup
- [ ] Navigate to Configuration → Backup/Restore
- [ ] Verify "Import SQLite Backup" button is present
- [ ] Click "Import SQLite Backup" button
- [ ] Select the `v5.0.0-sqlite-backup.db` file
- [ ] Confirm import operation
- [ ] Wait for import to complete
- [ ] Record import duration
- [ ] Verify success message appears

**Expected Result:** SQLite backup imports successfully

#### Step 5.6: Verify Restored Data
- [ ] Navigate to Configuration page
- [ ] Verify all configuration from backup is restored
- [ ] Verify the items added in Step 5.2 are NOT present (backup is from before those changes)
- [ ] Navigate to Generated Names Log
- [ ] Verify generated names match the backup state
- [ ] Test CRUD operations to ensure database is fully functional

**Expected Result:** Data restored to exact state of backup

---

### Phase 6: Azure Validation Feature Testing
**Objective:** Verify Azure Name Validation works with SQLite storage

#### Step 6.1: Enable Azure Validation
- [ ] Navigate to Configuration → Azure Validation
- [ ] Verify Azure validation settings are present
- [ ] Enable Azure Tenant Name Validation
- [ ] Configure Azure credentials (if not already configured)
- [ ] Save settings

**Expected Result:** Azure validation enabled successfully

#### Step 6.2: Test Azure Validation
- [ ] Navigate to Generate page
- [ ] Generate a name for a resource that supports Azure validation
- [ ] Verify validation check occurs (loading message appears)
- [ ] Verify validation result is displayed
- [ ] Test with both available and unavailable names
- [ ] Verify results are saved to Generated Names Log with validation status

**Expected Result:** Azure validation works correctly with SQLite storage

---

## Test Results Summary

### Environment Information
- **Test Date:** _______________
- **Tester:** _______________
- **Azure App Service:** azurenamingtool-dev
- **Resource Group:** soltisweb
- **Region:** East US 2

### Phase Results
| Phase | Status | Duration | Notes |
|-------|--------|----------|-------|
| Phase 1: v4.3.2 Baseline | ⬜ Pass / ⬜ Fail | _______ | |
| Phase 2: v5.0.0 Upgrade | ⬜ Pass / ⬜ Fail | _______ | |
| Phase 3: JSON Import | ⬜ Pass / ⬜ Fail | _______ | |
| Phase 4: SQLite Migration | ⬜ Pass / ⬜ Fail | _______ | |
| Phase 5: Backup/Restore | ⬜ Pass / ⬜ Fail | _______ | |
| Phase 6: Azure Validation | ⬜ Pass / ⬜ Fail | _______ | |

### Data Integrity Verification
| Metric | v4.3.2 Count | Post-Migration Count | Match? |
|--------|--------------|----------------------|--------|
| Resource Types | _______ | _______ | ⬜ Yes / ⬜ No |
| Resource Locations | _______ | _______ | ⬜ Yes / ⬜ No |
| Resource Environments | _______ | _______ | ⬜ Yes / ⬜ No |
| Resource Orgs | _______ | _______ | ⬜ Yes / ⬜ No |
| Resource Units/Depts | _______ | _______ | ⬜ Yes / ⬜ No |
| Resource Functions | _______ | _______ | ⬜ Yes / ⬜ No |
| Custom Components | _______ | _______ | ⬜ Yes / ⬜ No |
| Generated Names | _______ | _______ | ⬜ Yes / ⬜ No |

### Issues Found
| Issue # | Phase | Description | Severity | Status |
|---------|-------|-------------|----------|--------|
| | | | | |

### Overall Test Result
⬜ **PASS** - All phases completed successfully  
⬜ **FAIL** - Critical issues found (see Issues table)  
⬜ **PARTIAL** - Minor issues found, acceptable for release

### Sign-off
**Tester Signature:** _______________  
**Date:** _______________  
**Approved for Release:** ⬜ Yes / ⬜ No

---

## Notes and Observations
_Use this section to record any additional observations, performance notes, or recommendations._


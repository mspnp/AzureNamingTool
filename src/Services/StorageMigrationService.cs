using AzureNamingTool.Data;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for migrating configuration data from JSON files to SQLite database
    /// </summary>
    public class StorageMigrationService : IStorageMigrationService
    {
        private readonly ConfigurationDbContext _dbContext;
        private readonly ILogger<StorageMigrationService> _logger;
        private readonly string _settingsPath;
        private readonly string _backupBasePath;

        /// <summary>
        /// Initializes a new instance of the StorageMigrationService
        /// </summary>
        /// <param name="dbContext">Database context for SQLite operations</param>
        /// <param name="logger">Logger for migration operations</param>
        public StorageMigrationService(
            ConfigurationDbContext dbContext,
            ILogger<StorageMigrationService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            _backupBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        }

        /// <summary>
        /// Determines whether a migration from JSON to SQLite is needed
        /// </summary>
        /// <returns>True if migration is needed, false otherwise</returns>
        public async Task<bool> IsMigrationNeededAsync()
        {
            try
            {
                // Check if JSON files exist
                if (!Directory.Exists(_settingsPath))
                {
                    _logger.LogDebug("Settings directory does not exist");
                    return false;
                }

                var jsonFiles = Directory.GetFiles(_settingsPath, "*.json");
                if (jsonFiles.Length == 0)
                {
                    _logger.LogDebug("No JSON files found in settings directory");
                    return false;
                }

                // Check if SQLite database has any data
                var hasData = await _dbContext.ResourceTypes.AnyAsync() ||
                              await _dbContext.ResourceLocations.AnyAsync() ||
                              await _dbContext.ResourceEnvironments.AnyAsync();

                _logger.LogInformation("Migration check: JSON files={JsonFiles}, SQLite has data={HasData}",
                    jsonFiles.Length, hasData);

                // Migration needed if JSON files exist and SQLite is empty
                return jsonFiles.Length > 0 && !hasData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if migration is needed");
                return false;
            }
        }

        /// <summary>
        /// Creates a timestamped backup of current JSON configuration files
        /// </summary>
        /// <returns>Path to the backup directory</returns>
        public Task<string> BackupCurrentDataAsync()
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(_backupBasePath, $"backup_{timestamp}");

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                _logger.LogInformation("Creating backup at {BackupPath}", backupPath);

                // Copy all JSON files
                if (Directory.Exists(_settingsPath))
                {
                    var jsonFiles = Directory.GetFiles(_settingsPath, "*.json");
                    foreach (var file in jsonFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var destFile = Path.Combine(backupPath, fileName);
                        File.Copy(file, destFile, overwrite: true);
                        _logger.LogDebug("Backed up {FileName}", fileName);
                    }

                    _logger.LogInformation("Backup completed: {FileCount} files copied to {BackupPath}",
                        jsonFiles.Length, backupPath);
                }

                return Task.FromResult(backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                throw new InvalidOperationException("Failed to create backup", ex);
            }
        }

        /// <summary>
        /// Migrates all configuration data from JSON files to SQLite
        /// </summary>
        /// <param name="backupPath">Optional backup path; if not provided, creates a new backup</param>
        /// <returns>Migration result with success status and details</returns>
        public async Task<MigrationResult> MigrateToSQLiteAsync(string? backupPath = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MigrationResult();

            try
            {
                _logger.LogInformation("Starting migration from JSON to SQLite");

                // Create backup if not provided
                if (string.IsNullOrEmpty(backupPath))
                {
                    backupPath = await BackupCurrentDataAsync();
                }
                result.BackupPath = backupPath;

                // Ensure database is created
                await _dbContext.Database.EnsureCreatedAsync();

                // Migrate each entity type (using plural filenames as they exist in settings)
                await MigrateEntityAsync<ResourceType>("resourcetypes.json", result);
                await MigrateEntityAsync<ResourceLocation>("resourcelocations.json", result);
                await MigrateEntityAsync<ResourceEnvironment>("resourceenvironments.json", result);
                await MigrateEntityAsync<ResourceOrg>("resourceorgs.json", result);
                await MigrateEntityAsync<ResourceProjAppSvc>("resourceprojappsvcs.json", result);
                await MigrateEntityAsync<ResourceUnitDept>("resourceunitdepts.json", result);
                await MigrateEntityAsync<ResourceFunction>("resourcefunctions.json", result);
                await MigrateEntityAsync<ResourceDelimiter>("resourcedelimiters.json", result);
                await MigrateEntityAsync<ResourceComponent>("resourcecomponents.json", result);
                await MigrateEntityAsync<CustomComponent>("customcomponents.json", result);
                await MigrateEntityAsync<AdminUser>("adminusers.json", result);
                await MigrateEntityAsync<AdminLogMessage>("adminlogmessages.json", result);
                await MigrateEntityAsync<GeneratedName>("generatednames.json", result);
                
                // Migrate Azure Validation settings (singleton entity)
                await MigrateAzureValidationSettingsAsync(result);

                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Migration completed successfully. {result.EntitiesMigrated} entities migrated in {result.Duration.TotalSeconds:F2} seconds.";

                _logger.LogInformation(result.Message);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.Duration = stopwatch.Elapsed;
                result.Message = $"Migration failed: {ex.Message}";
                result.Errors.Add(ex.ToString());

                _logger.LogError(ex, "Migration failed after {Duration}ms", stopwatch.ElapsedMilliseconds);

                // Attempt rollback
                if (!string.IsNullOrEmpty(backupPath))
                {
                    _logger.LogWarning("Attempting automatic rollback from {BackupPath}", backupPath);
                    await RollbackMigrationAsync(backupPath);
                }

                return result;
            }
        }

        private async Task MigrateEntityAsync<TEntity>(string fileName, MigrationResult result) where TEntity : class
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogDebug("File {FileName} does not exist, skipping", fileName);
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json) || json == "[]")
                {
                    _logger.LogDebug("File {FileName} is empty, skipping", fileName);
                    result.EntityCounts[typeof(TEntity).Name] = 0;
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var entities = JsonSerializer.Deserialize<List<TEntity>>(json, options);
                if (entities == null || entities.Count == 0)
                {
                    _logger.LogDebug("No entities found in {FileName}", fileName);
                    result.EntityCounts[typeof(TEntity).Name] = 0;
                    return;
                }

                // Add entities to database
                var dbSet = _dbContext.Set<TEntity>();
                await dbSet.AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();

                result.EntitiesMigrated += entities.Count;
                result.EntityCounts[typeof(TEntity).Name] = entities.Count;

                _logger.LogInformation("Migrated {Count} {EntityType} entities from {FileName}",
                    entities.Count, typeof(TEntity).Name, fileName);
            }
            catch (Exception ex)
            {
                var error = $"Failed to migrate {typeof(TEntity).Name} from {fileName}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Error migrating {EntityType}", typeof(TEntity).Name);
                throw new InvalidOperationException(error, ex);
            }
        }

        /// <summary>
        /// Migrates Azure Validation settings from JSON file to SQLite (singleton entity)
        /// </summary>
        /// <param name="result">Migration result to update</param>
        private async Task MigrateAzureValidationSettingsAsync(MigrationResult result)
        {
            const string fileName = "azurevalidationsettings.json";
            
            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogDebug("File {FileName} does not exist, skipping Azure Validation settings migration", fileName);
                    result.EntityCounts[nameof(AzureValidationSettings)] = 0;
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json) || json == "{}")
                {
                    _logger.LogDebug("File {FileName} is empty, skipping Azure Validation settings migration", fileName);
                    result.EntityCounts[nameof(AzureValidationSettings)] = 0;
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Try to deserialize as array first (legacy format), then as single object
                AzureValidationSettings? settings = null;
                
                // Try array format first
                var settingsArray = JsonSerializer.Deserialize<List<AzureValidationSettings>>(json, options);
                if (settingsArray != null && settingsArray.Count > 0)
                {
                    settings = settingsArray[0];
                    _logger.LogInformation("Deserialized Azure Validation settings from array format. Strategy: {Strategy}, SubscriptionIds count: {Count}", 
                        settings.ConflictResolution?.Strategy, settings.SubscriptionIds?.Count ?? 0);
                }
                else
                {
                    // Try single object format
                    settings = JsonSerializer.Deserialize<AzureValidationSettings>(json, options);
                    if (settings != null)
                    {
                        _logger.LogInformation("Deserialized Azure Validation settings from single object format. Strategy: {Strategy}, SubscriptionIds count: {Count}", 
                            settings.ConflictResolution?.Strategy, settings.SubscriptionIds?.Count ?? 0);
                    }
                }

                if (settings == null)
                {
                    _logger.LogDebug("No Azure Validation settings found in {FileName}", fileName);
                    result.EntityCounts[nameof(AzureValidationSettings)] = 0;
                    return;
                }

                // Ensure ID is set to 1 for singleton pattern
                settings.Id = 1;
                
                _logger.LogInformation("Before saving to database - Strategy: {Strategy}, SubscriptionIds: {Subs}", 
                    settings.ConflictResolution?.Strategy, string.Join(", ", settings.SubscriptionIds ?? new List<string>()));

                // Add to database
                _dbContext.AzureValidationSettings.Add(settings);
                await _dbContext.SaveChangesAsync();
                
                // Read back to verify
                var savedSettings = await _dbContext.AzureValidationSettings.FindAsync(1L);
                _logger.LogInformation("After saving to database - Strategy: {Strategy}, SubscriptionIds: {Subs}", 
                    savedSettings?.ConflictResolution?.Strategy, string.Join(", ", savedSettings?.SubscriptionIds ?? new List<string>()));

                result.EntitiesMigrated += 1;
                result.EntityCounts[nameof(AzureValidationSettings)] = 1;

                _logger.LogInformation("Migrated Azure Validation settings from {FileName}", fileName);
            }
            catch (Exception ex)
            {
                var error = $"Failed to migrate Azure Validation settings from {fileName}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Error migrating Azure Validation settings");
                // Don't throw - allow migration to continue even if Azure Validation settings fail
            }
        }

        /// <summary>
        /// Validates that migrated data matches the source JSON files
        /// </summary>
        /// <returns>Validation result with entity count comparisons</returns>
        public async Task<ValidationResult> ValidateMigrationAsync()
        {
            var validation = new ValidationResult { IsValid = true };

            try
            {
                _logger.LogInformation("Validating migration");

                // Validate each entity type
                await ValidateEntityAsync<ResourceType>("resourcetypes.json", validation);
                await ValidateEntityAsync<ResourceLocation>("resourcelocations.json", validation);
                await ValidateEntityAsync<ResourceEnvironment>("resourceenvironments.json", validation);
                await ValidateEntityAsync<ResourceOrg>("resourceorgs.json", validation);
                await ValidateEntityAsync<ResourceProjAppSvc>("resourceprojappsvcs.json", validation);
                await ValidateEntityAsync<ResourceUnitDept>("resourceunitdepts.json", validation);
                await ValidateEntityAsync<ResourceFunction>("resourcefunctions.json", validation);
                await ValidateEntityAsync<ResourceDelimiter>("resourcedelimiters.json", validation);
                await ValidateEntityAsync<ResourceComponent>("resourcecomponents.json", validation);
                await ValidateEntityAsync<CustomComponent>("customcomponents.json", validation);
                await ValidateEntityAsync<AdminUser>("adminusers.json", validation);
                await ValidateEntityAsync<AdminLogMessage>("adminlogmessages.json", validation);
                await ValidateEntityAsync<GeneratedName>("generatednames.json", validation);
                
                // Validate Azure Validation settings (singleton)
                await ValidateAzureValidationSettingsAsync(validation);

                validation.Message = validation.IsValid
                    ? "Validation successful - all entities match"
                    : "Validation failed - discrepancies found";

                _logger.LogInformation(validation.Message);
                return validation;
            }
            catch (Exception ex)
            {
                validation.IsValid = false;
                validation.Message = $"Validation error: {ex.Message}";
                _logger.LogError(ex, "Error during validation");
                return validation;
            }
        }

        private async Task ValidateEntityAsync<TEntity>(string fileName, ValidationResult validation) where TEntity : class
        {
            var detail = new ValidationDetail();
            var entityName = typeof(TEntity).Name;

            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);

                // Get source count from JSON
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    if (!string.IsNullOrWhiteSpace(json) && json != "[]")
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true
                        };
                        var entities = JsonSerializer.Deserialize<List<TEntity>>(json, options);
                        detail.SourceCount = entities?.Count ?? 0;
                    }
                }

                // Get target count from SQLite
                var dbSet = _dbContext.Set<TEntity>();
                detail.TargetCount = await dbSet.CountAsync();

                detail.Matches = detail.SourceCount == detail.TargetCount;
                
                if (!detail.Matches)
                {
                    detail.Discrepancies.Add($"Count mismatch: JSON={detail.SourceCount}, SQLite={detail.TargetCount}");
                    validation.IsValid = false;
                }

                validation.EntityValidation[entityName] = detail;

                _logger.LogDebug("Validation for {EntityType}: Source={SourceCount}, Target={TargetCount}, Matches={Matches}",
                    entityName, detail.SourceCount, detail.TargetCount, detail.Matches);
            }
            catch (Exception ex)
            {
                detail.Discrepancies.Add($"Validation error: {ex.Message}");
                validation.EntityValidation[entityName] = detail;
                validation.IsValid = false;
                _logger.LogError(ex, "Error validating {EntityType}", entityName);
            }
        }

        /// <summary>
        /// Validates Azure Validation settings migration (singleton entity)
        /// </summary>
        /// <param name="validation">Validation result to update</param>
        private async Task ValidateAzureValidationSettingsAsync(ValidationResult validation)
        {
            var detail = new ValidationDetail();
            const string fileName = "azurevalidationsettings.json";
            const string entityName = nameof(AzureValidationSettings);

            try
            {
                var filePath = Path.Combine(_settingsPath, fileName);

                // Get source count from JSON (should be 1 or 0)
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    if (!string.IsNullOrWhiteSpace(json) && json != "{}")
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        AzureValidationSettings? settings = null;
                        
                        // Try array format first (settings stored as [{...}])
                        var settingsArray = JsonSerializer.Deserialize<List<AzureValidationSettings>>(json, options);
                        if (settingsArray != null && settingsArray.Count > 0)
                        {
                            settings = settingsArray[0];
                        }
                        else
                        {
                            // Fallback to single object format
                            settings = JsonSerializer.Deserialize<AzureValidationSettings>(json, options);
                        }
                        
                        detail.SourceCount = settings != null ? 1 : 0;
                    }
                }

                // Get target count from SQLite (should be 1 or 0)
                detail.TargetCount = await _dbContext.AzureValidationSettings.CountAsync();

                detail.Matches = detail.SourceCount == detail.TargetCount;
                
                if (!detail.Matches)
                {
                    detail.Discrepancies.Add($"Count mismatch: JSON={detail.SourceCount}, SQLite={detail.TargetCount}");
                    validation.IsValid = false;
                }

                validation.EntityValidation[entityName] = detail;

                _logger.LogDebug("Validation for {EntityType}: Source={SourceCount}, Target={TargetCount}, Matches={Matches}",
                    entityName, detail.SourceCount, detail.TargetCount, detail.Matches);
            }
            catch (Exception ex)
            {
                detail.Discrepancies.Add($"Validation error: {ex.Message}");
                validation.EntityValidation[entityName] = detail;
                validation.IsValid = false;
                _logger.LogError(ex, "Error validating {EntityType}", entityName);
            }
        }

        /// <summary>
        /// Rolls back a failed migration by deleting the SQLite database and restoring from backup
        /// </summary>
        /// <param name="backupPath">Path to the backup directory to restore from</param>
        /// <returns>True if rollback was successful, false otherwise</returns>
        public async Task<bool> RollbackMigrationAsync(string backupPath)
        {
            try
            {
                _logger.LogWarning("Rolling back migration from {BackupPath}", backupPath);

                if (!Directory.Exists(backupPath))
                {
                    _logger.LogError("Backup directory {BackupPath} does not exist", backupPath);
                    return false;
                }

                // Clear SQLite database
                await _dbContext.Database.EnsureDeletedAsync();
                _logger.LogInformation("SQLite database cleared");

                // Restore JSON files from backup
                var backupFiles = Directory.GetFiles(backupPath, "*.json");
                foreach (var file in backupFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(_settingsPath, fileName);
                    File.Copy(file, destFile, overwrite: true);
                    _logger.LogDebug("Restored {FileName}", fileName);
                }

                _logger.LogInformation("Rollback completed: {FileCount} files restored", backupFiles.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rollback");
                return false;
            }
        }

        /// <summary>
        /// Gets the current migration status including file existence and migration state
        /// </summary>
        /// <returns>Migration status information</returns>
        public async Task<MigrationStatus> GetMigrationStatusAsync()
        {
            var status = new MigrationStatus
            {
                CurrentProvider = "Unknown",
                JsonFilesExist = Directory.Exists(_settingsPath) && Directory.GetFiles(_settingsPath, "*.json").Length > 0,
                SQLiteDatabaseExists = await _dbContext.Database.CanConnectAsync()
            };

            try
            {
                if (status.SQLiteDatabaseExists)
                {
                    var hasData = await _dbContext.ResourceTypes.AnyAsync();
                    status.IsMigrated = hasData;
                    status.CurrentProvider = "SQLite";
                }
                else if (status.JsonFilesExist)
                {
                    status.CurrentProvider = "FileSystem";
                }

                _logger.LogDebug("Migration status: Provider={Provider}, Migrated={Migrated}, JSON={JsonExists}, SQLite={SQLiteExists}",
                    status.CurrentProvider, status.IsMigrated, status.JsonFilesExist, status.SQLiteDatabaseExists);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status");
                return status;
            }
        }

        /// <summary>
        /// Loads repository JSON files into SQLite database for new installations
        /// </summary>
        /// <returns>Migration result with success status and details</returns>
        public async Task<MigrationResult> LoadRepositoryDataIntoSQLiteAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MigrationResult();

            try
            {
                _logger.LogInformation("Loading repository data into SQLite for new installation");

                // Repository path (contains default JSON files)
                var repositoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository");

                if (!Directory.Exists(repositoryPath))
                {
                    result.Success = false;
                    result.Message = "Repository folder not found";
                    _logger.LogError("Repository folder not found at {RepositoryPath}", repositoryPath);
                    return result;
                }

                // Ensure database is created
                await _dbContext.Database.EnsureCreatedAsync();

                // Load each entity type from repository folder (using plural filenames)
                await LoadEntityFromRepositoryAsync<ResourceType>("resourcetypes.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceLocation>("resourcelocations.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceEnvironment>("resourceenvironments.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceOrg>("resourceorgs.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceProjAppSvc>("resourceprojappsvcs.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceUnitDept>("resourceunitdepts.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceFunction>("resourcefunctions.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceDelimiter>("resourcedelimiters.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<ResourceComponent>("resourcecomponents.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<CustomComponent>("customcomponents.json", repositoryPath, result);
                await LoadEntityFromRepositoryAsync<AdminUser>("adminusers.json", repositoryPath, result);

                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Repository data loaded successfully. {result.EntitiesMigrated} entities loaded in {result.Duration.TotalSeconds:F2} seconds.";

                _logger.LogInformation(result.Message);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.Duration = stopwatch.Elapsed;
                result.Message = $"Failed to load repository data: {ex.Message}";
                result.Errors.Add(ex.ToString());

                _logger.LogError(ex, "Failed to load repository data after {Duration}ms", stopwatch.ElapsedMilliseconds);
                return result;
            }
        }

        private async Task LoadEntityFromRepositoryAsync<TEntity>(string fileName, string repositoryPath, MigrationResult result) where TEntity : class
        {
            try
            {
                var filePath = Path.Combine(repositoryPath, fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogDebug("Repository file {FileName} does not exist, skipping", fileName);
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json) || json == "[]")
                {
                    _logger.LogDebug("Repository file {FileName} is empty, skipping", fileName);
                    return;
                }

                var entities = JsonSerializer.Deserialize<List<TEntity>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (entities == null || !entities.Any())
                {
                    _logger.LogDebug("No entities deserialized from {FileName}", fileName);
                    return;
                }

                // Add entities to the database
                _dbContext.Set<TEntity>().AddRange(entities);
                await _dbContext.SaveChangesAsync();

                result.EntitiesMigrated += entities.Count;
                _logger.LogDebug("Loaded {Count} entities from {FileName}", entities.Count, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading entities from {FileName}", fileName);
                result.Errors.Add($"{fileName}: {ex.Message}");
                throw;
            }
        }
    }
}

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for migrating configuration data between storage providers
    /// </summary>
    public interface IStorageMigrationService
    {
        /// <summary>
        /// Detects if JSON files exist and migration is needed
        /// </summary>
        Task<bool> IsMigrationNeededAsync();

        /// <summary>
        /// Backs up current JSON configuration data
        /// </summary>
        /// <returns>Path to backup directory</returns>
        Task<string> BackupCurrentDataAsync();

        /// <summary>
        /// Migrates all configuration data from JSON to SQLite
        /// </summary>
        /// <param name="backupPath">Optional backup path for rollback</param>
        Task<MigrationResult> MigrateToSQLiteAsync(string? backupPath = null);

        /// <summary>
        /// Validates that migrated data matches source data
        /// </summary>
        Task<ValidationResult> ValidateMigrationAsync();

        /// <summary>
        /// Rolls back migration by restoring from backup
        /// </summary>
        Task<bool> RollbackMigrationAsync(string backupPath);

        /// <summary>
        /// Gets migration status and history
        /// </summary>
        Task<MigrationStatus> GetMigrationStatusAsync();

        /// <summary>
        /// Loads repository JSON files into SQLite database for new installations
        /// </summary>
        Task<MigrationResult> LoadRepositoryDataIntoSQLiteAsync();
    }

    /// <summary>
    /// Result of a migration operation
    /// </summary>
    public class MigrationResult
    {
        /// <summary>
        /// Gets or sets whether the migration was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the migration result message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the total number of entities migrated
        /// </summary>
        public int EntitiesMigrated { get; set; }
        
        /// <summary>
        /// Gets or sets the duration of the migration
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the count of entities by type
        /// </summary>
        public Dictionary<string, int> EntityCounts { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of errors encountered
        /// </summary>
        public List<string> Errors { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the path to the backup directory
        /// </summary>
        public string? BackupPath { get; set; }
    }

    /// <summary>
    /// Result of migration validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets whether the validation passed
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Gets or sets the validation result message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the validation details by entity type
        /// </summary>
        public Dictionary<string, ValidationDetail> EntityValidation { get; set; } = new();
    }

    /// <summary>
    /// Detailed validation information for an entity type
    /// </summary>
    public class ValidationDetail
    {
        /// <summary>
        /// Gets or sets the count of entities in the source (JSON)
        /// </summary>
        public int SourceCount { get; set; }
        
        /// <summary>
        /// Gets or sets the count of entities in the target (SQLite)
        /// </summary>
        public int TargetCount { get; set; }
        
        /// <summary>
        /// Gets or sets whether the counts match
        /// </summary>
        public bool Matches { get; set; }
        
        /// <summary>
        /// Gets or sets the list of discrepancies found
        /// </summary>
        public List<string> Discrepancies { get; set; } = new();
    }

    /// <summary>
    /// Migration status information
    /// </summary>
    public class MigrationStatus
    {
        /// <summary>
        /// Gets or sets whether data has been migrated to SQLite
        /// </summary>
        public bool IsMigrated { get; set; }
        
        /// <summary>
        /// Gets or sets the date of the last migration
        /// </summary>
        public DateTime? LastMigrationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the current storage provider name
        /// </summary>
        public string CurrentProvider { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether JSON files exist in the settings folder
        /// </summary>
        public bool JsonFilesExist { get; set; }
        
        /// <summary>
        /// Gets or sets whether the SQLite database exists
        /// </summary>
        public bool SQLiteDatabaseExists { get; set; }
    }
}

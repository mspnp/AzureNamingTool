namespace AzureNamingTool.Models
{
    /// <summary>
    /// Configuration options for storage provider selection
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Storage provider type: "FileSystem" (JSON) or "SQLite"
        /// Default: FileSystem (maintains backward compatibility)
        /// </summary>
        public string Provider { get; set; } = "FileSystem";

        /// <summary>
        /// Path to SQLite database file (relative or absolute)
        /// Default: settings/azurenamingtool.db
        /// </summary>
        public string DatabasePath { get; set; } = "settings/azurenamingtool.db";

        /// <summary>
        /// Enable automatic migration from JSON to SQLite on startup
        /// Only applies when Provider=SQLite and JSON files exist
        /// Default: false (manual migration required)
        /// </summary>
        public bool EnableAutoMigration { get; set; } = false;

        /// <summary>
        /// Number of days to retain backup files after migration
        /// Default: 30 days
        /// </summary>
        public int BackupRetentionDays { get; set; } = 30;
    }
}

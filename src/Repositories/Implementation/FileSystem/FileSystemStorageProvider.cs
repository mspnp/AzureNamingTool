#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Repositories.Interfaces;

namespace AzureNamingTool.Repositories.Implementation.FileSystem
{
    /// <summary>
    /// File system storage provider (DEFAULT IMPLEMENTATION)
    /// Manages health and availability of JSON file-based storage
    /// </summary>
    public class FileSystemStorageProvider : IStorageProvider
    {
        private readonly ILogger<FileSystemStorageProvider> _logger;
        private readonly string _settingsPath;

        public string ProviderName => "FileSystem (JSON)";

        public FileSystemStorageProvider(ILogger<FileSystemStorageProvider> logger)
        {
            _logger = logger;
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
        }

        public Task<bool> IsAvailableAsync()
        {
            try
            {
                var exists = Directory.Exists(_settingsPath);
                _logger.LogDebug("Settings directory exists: {Exists}, Path: {Path}", exists, _settingsPath);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking storage availability");
                return Task.FromResult(false);
            }
        }

        public Task InitializeAsync()
        {
            try
            {
                if (!Directory.Exists(_settingsPath))
                {
                    Directory.CreateDirectory(_settingsPath);
                    _logger.LogInformation("Created settings directory: {Path}", _settingsPath);
                }
                else
                {
                    _logger.LogDebug("Settings directory already exists: {Path}", _settingsPath);
                }
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing file system storage");
                throw new InvalidOperationException("Failed to initialize file system storage", ex);
            }
        }

        public async Task<StorageHealthStatus> GetHealthAsync()
        {
            try
            {
                var isAvailable = await IsAvailableAsync();
                
                if (!isAvailable)
                {
                    return new StorageHealthStatus(
                        false,
                        ProviderName,
                        $"Settings directory not accessible: {_settingsPath}");
                }

                // Check if we can write
                var testFile = Path.Combine(_settingsPath, ".health-check");
                try
                {
                    await File.WriteAllTextAsync(testFile, DateTime.UtcNow.ToString("O"));
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cannot write to settings directory");
                    return new StorageHealthStatus(
                        false,
                        ProviderName,
                        "Settings directory is not writable",
                        new Dictionary<string, object>
                        {
                            ["SettingsPath"] = _settingsPath,
                            ["Error"] = ex.Message
                        });
                }

                // Get metadata about the storage
                var metadata = new Dictionary<string, object>
                {
                    ["SettingsPath"] = _settingsPath,
                    ["DirectoryExists"] = Directory.Exists(_settingsPath),
                    ["FileCount"] = Directory.Exists(_settingsPath) 
                        ? Directory.GetFiles(_settingsPath, "*.json").Length 
                        : 0,
                    ["LastChecked"] = DateTime.UtcNow
                };

                if (Directory.Exists(_settingsPath))
                {
                    var dirInfo = new DirectoryInfo(_settingsPath);
                    metadata["CreatedDate"] = dirInfo.CreationTimeUtc;
                    metadata["LastModified"] = dirInfo.LastWriteTimeUtc;
                }

                return new StorageHealthStatus(
                    true,
                    ProviderName,
                    "File system storage is healthy", 
                    metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage health check failed");
                return new StorageHealthStatus(
                    false,
                    ProviderName,
                    $"Health check failed: {ex.Message}",
                    new Dictionary<string, object>
                    {
                        ["Exception"] = ex.GetType().Name,
                        ["Message"] = ex.Message
                    });
            }
        }
    }
}

#pragma warning restore CS1591
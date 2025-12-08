namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Storage provider health status
    /// </summary>
    public record StorageHealthStatus(
        bool IsHealthy,
        string ProviderName,
        string Message, 
        Dictionary<string, object>? Metadata = null);

    /// <summary>
    /// Defines the storage provider abstraction
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Get the storage provider name
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Check if storage is available and accessible
        /// </summary>
        /// <returns>True if storage is available</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Initialize storage provider
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Get storage health status
        /// </summary>
        /// <returns>Health status with metadata</returns>
        Task<StorageHealthStatus> GetHealthAsync();
    }
}

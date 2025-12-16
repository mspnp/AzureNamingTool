using AzureNamingTool.Repositories.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AzureNamingTool.HealthChecks
{
    /// <summary>
    /// Health check for storage provider availability and responsiveness.
    /// </summary>
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger<StorageHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageHealthCheck"/> class.
        /// </summary>
        /// <param name="storageProvider">The storage provider to check.</param>
        /// <param name="logger">The logger instance.</param>
        public StorageHealthCheck(
            IStorageProvider storageProvider,
            ILogger<StorageHealthCheck> logger)
        {
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the health check for the storage provider.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the health check result.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Check storage provider health
                var healthStatus = await _storageProvider.GetHealthAsync();
                
                var duration = DateTime.UtcNow - startTime;
                
                if (healthStatus.IsHealthy)
                {
                    var data = new Dictionary<string, object>
                    {
                        { "provider", healthStatus.ProviderName },
                        { "responseDuration", $"{duration.TotalMilliseconds}ms" },
                        { "message", healthStatus.Message ?? "Storage is healthy" }
                    };

                    _logger.LogDebug("Storage health check passed: {Provider} in {Duration}ms", 
                        healthStatus.ProviderName, duration.TotalMilliseconds);

                    return HealthCheckResult.Healthy(
                        $"Storage provider '{healthStatus.ProviderName}' is healthy",
                        data);
                }
                else
                {
                    _logger.LogWarning("Storage health check failed: {Message}", healthStatus.Message);

                    return HealthCheckResult.Unhealthy(
                        healthStatus.Message ?? "Storage provider is not healthy");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage health check threw an exception");

                return HealthCheckResult.Unhealthy(
                    "Storage health check failed",
                    ex);
            }
        }
    }
}

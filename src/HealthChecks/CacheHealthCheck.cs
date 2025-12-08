using AzureNamingTool.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AzureNamingTool.HealthChecks
{
    /// <summary>
    /// Health check for cache service availability and responsiveness.
    /// </summary>
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheHealthCheck> _logger;
        private const string HealthCheckKey = "_healthcheck_cache_test";

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHealthCheck"/> class.
        /// </summary>
        /// <param name="cacheService">The cache service to check.</param>
        /// <param name="memoryCache">The underlying memory cache.</param>
        /// <param name="logger">The logger instance.</param>
        public CacheHealthCheck(
            ICacheService cacheService,
            IMemoryCache memoryCache,
            ILogger<CacheHealthCheck> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the health check for the cache service.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the health check result.</returns>
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var testValue = Guid.NewGuid().ToString();

                // Test cache write
                _cacheService.SetCacheObject(HealthCheckKey, testValue, expirationMinutes: 1);

                // Test cache read
                var retrievedValue = _cacheService.GetCacheObject<string>(HealthCheckKey);

                // Test cache invalidation
                _cacheService.InvalidateCacheObject(HealthCheckKey);

                var duration = DateTime.UtcNow - startTime;

                if (retrievedValue == testValue)
                {
                    var data = new Dictionary<string, object>
                    {
                        { "cacheType", "MemoryCache" },
                        { "responseDuration", $"{duration.TotalMilliseconds}ms" },
                        { "operations", "Set, Get, Invalidate" }
                    };

                    _logger.LogDebug("Cache health check passed in {Duration}ms", duration.TotalMilliseconds);

                    return Task.FromResult(HealthCheckResult.Healthy(
                        "Cache is healthy and responsive",
                        data));
                }
                else
                {
                    _logger.LogWarning("Cache health check failed: Retrieved value does not match");

                    return Task.FromResult(HealthCheckResult.Degraded(
                        "Cache operations are not working correctly"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache health check threw an exception");

                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Cache health check failed",
                    ex));
            }
        }
    }
}

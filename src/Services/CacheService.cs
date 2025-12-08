#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Cache service implementation using IMemoryCache
    /// Replaces static MemoryCache.Default usage throughout the application
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T? GetCacheObject<T>(string cacheKey) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(cacheKey, out T? cachedData))
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    return cachedData;
                }

                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache object for key: {CacheKey}", cacheKey);
                return null;
            }
        }

        public void SetCacheObject<T>(string cacheKey, T data, int expirationMinutes = 60) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                    Priority = CacheItemPriority.Normal
                };

                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _cacheKeys.TryRemove(key.ToString()!, out _);
                    _logger.LogDebug("Cache evicted: {Key}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(cacheKey, data, cacheOptions);
                _cacheKeys.TryAdd(cacheKey, 0);

                _logger.LogDebug("Cache set for key: {CacheKey}, Expiration: {Minutes} minutes",
                    cacheKey, expirationMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache object for key: {CacheKey}", cacheKey);
            }
        }

        public void InvalidateCacheObject(string cacheKey)
        {
            try
            {
                _memoryCache.Remove(cacheKey);
                _cacheKeys.TryRemove(cacheKey, out _);
                _logger.LogInformation("Cache invalidated for key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
            }
        }

        public void ClearAllCache()
        {
            try
            {
                foreach (var key in _cacheKeys.Keys.ToList())
                {
                    _memoryCache.Remove(key);
                }
                _cacheKeys.Clear();
                _logger.LogInformation("All cache cleared, {Count} entries removed", _cacheKeys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache");
            }
        }

        public bool Exists(string cacheKey)
        {
            return _cacheKeys.ContainsKey(cacheKey);
        }
    }
}

#pragma warning restore CS1591
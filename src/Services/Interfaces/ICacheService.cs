namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Service for managing application cache
    /// Replaces static MemoryCache.Default with dependency-injected IMemoryCache
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get cached object by key
        /// </summary>
        /// <typeparam name="T">Type of cached object</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>Cached object if found, null otherwise</returns>
        T? GetCacheObject<T>(string cacheKey) where T : class;

        /// <summary>
        /// Set cache object with expiration
        /// </summary>
        /// <typeparam name="T">Type of object to cache</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="data">Data to cache</param>
        /// <param name="expirationMinutes">Expiration time in minutes (default 60)</param>
        void SetCacheObject<T>(string cacheKey, T data, int expirationMinutes = 60) where T : class;

        /// <summary>
        /// Remove specific cache entry
        /// </summary>
        /// <param name="cacheKey">Cache key to remove</param>
        void InvalidateCacheObject(string cacheKey);

        /// <summary>
        /// Clear all cache entries
        /// </summary>
        void ClearAllCache();

        /// <summary>
        /// Check if cache key exists
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>True if key exists, false otherwise</returns>
        bool Exists(string cacheKey);
    }
}

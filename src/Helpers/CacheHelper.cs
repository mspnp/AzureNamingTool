using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Runtime.Caching;
using System.Text;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for caching operations.
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// Retrieves an object from the cache based on the specified cache key.
        /// </summary>
        /// <param name="cachekey">The cache key.</param>
        /// <returns>The cached object, or null if not found.</returns>
        public static object? GetCacheObject(string cachekey)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var encodedCache = memoryCache.Get(cachekey);
                if (encodedCache == null)
                {
                    return null;
                }
                else
                {
                    return (object)encodedCache;
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }

        /// <summary>
        /// Sets an object in the cache with the specified cache key and data.
        /// </summary>
        /// <param name="cachekey">The cache key.</param>
        /// <param name="cachedata">The data to be cached.</param>
        public static void SetCacheObject(string cachekey, object cachedata)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600.0),

                };
                memoryCache.Set(cachekey, cachedata, cacheItemPolicy);
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        /// <summary>
        /// Invalidates the cached object with the specified cache key.
        /// </summary>
        /// <param name="cachekey">The cache key.</param>
        public static void InvalidateCacheObject(string cachekey)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                memoryCache.Remove(cachekey);
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all the cached data.
        /// </summary>
        /// <returns>A string containing all the cached data.</returns>
        public static string GetAllCacheData()
        {
            StringBuilder data = new();
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
                foreach (var key in cacheKeys.OrderBy(x => x))
                {
                    data.Append("<p><span class=\"fw-bold\">" + key + "</span></p><div class=\"alert alert-secondary\" style=\"word-wrap:break-word;\">" + MemoryCache.Default[key].ToString() + "</div>");
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                data.Append("<p><span class=\"fw-bold\">No data currently cached.</span></p>");
            }
            return data.ToString();
        }

        /// <summary>
        /// Clears all the cached data.
        /// </summary>
        public static void ClearAllCache()
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                List<string> cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
                foreach (string cacheKey in cacheKeys)
                {
                    memoryCache.Remove(cacheKey);
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }
    }
}

namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for configuration entities stored in JSON files
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IConfigurationRepository<T> where T : class
    {
        /// <summary>
        /// Get all items from storage
        /// </summary>
        /// <returns>Collection of all items</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get a specific item by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>Item if found, null otherwise</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Save a single item (create or update)
        /// </summary>
        /// <param name="item">Item to save</param>
        Task SaveAsync(T item);

        /// <summary>
        /// Save all items (replaces entire collection)
        /// </summary>
        /// <param name="items">Items to save</param>
        Task SaveAllAsync(IEnumerable<T> items);

        /// <summary>
        /// Delete an item by ID
        /// </summary>
        /// <param name="id">Item ID to delete</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Check if an item exists by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True if item exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);
    }
}

using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource functions.
/// </summary>
public interface IResourceFunctionService
{
    /// <summary>
    /// Retrieves all resource functions.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource function by ID.
    /// </summary>
    /// <param name="id">The ID of the resource function.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource function.
    /// </summary>
    /// <param name="item">The resource function to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceFunction item);

    /// <summary>
    /// Deletes a resource function by ID.
    /// </summary>
    /// <param name="id">The ID of the resource function to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource functions.
    /// </summary>
    /// <param name="items">The list of resource functions to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceFunction> items);

    /// <summary>
    /// Updates the sort order of resource functions without resetting IDs.
    /// </summary>
    /// <param name="items">The list of resource functions with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceFunction> items);
}

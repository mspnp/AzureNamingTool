using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource environments.
/// </summary>
public interface IResourceEnvironmentService
{
    /// <summary>
    /// Retrieves all resource environments.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource environment by ID.
    /// </summary>
    /// <param name="id">The ID of the resource environment.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource environment.
    /// </summary>
    /// <param name="item">The resource environment to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceEnvironment item);

    /// <summary>
    /// Deletes a resource environment by ID.
    /// </summary>
    /// <param name="id">The ID of the resource environment to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource environments.
    /// </summary>
    /// <param name="items">The list of resource environments to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceEnvironment> items);

    /// <summary>
    /// Updates the sort order of resource environments without normalization.
    /// </summary>
    /// <param name="items">The list of resource environments with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceEnvironment> items);
}

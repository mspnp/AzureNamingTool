using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource components.
/// </summary>
public interface IResourceComponentService
{
    /// <summary>
    /// Retrieves all resource components.
    /// </summary>
    /// <param name="admin">Flag indicating whether to include all components or only enabled ones.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin);

    /// <summary>
    /// Retrieves a specific resource component by ID.
    /// </summary>
    /// <param name="id">The ID of the resource component.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource component.
    /// </summary>
    /// <param name="item">The resource component to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceComponent item);

    /// <summary>
    /// Deletes a resource component by ID.
    /// </summary>
    /// <param name="id">The ID of the resource component to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource components.
    /// </summary>
    /// <param name="items">The list of resource components to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceComponent> items);

    /// <summary>
    /// Updates the sort order of resource components without normalization.
    /// </summary>
    /// <param name="items">The list of resource components with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceComponent> items);
}

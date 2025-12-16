using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource organizations.
/// </summary>
public interface IResourceOrgService
{
    /// <summary>
    /// Retrieves all resource organizations.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource organization by ID.
    /// </summary>
    /// <param name="id">The ID of the resource organization.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource organization.
    /// </summary>
    /// <param name="item">The resource organization to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceOrg item);

    /// <summary>
    /// Deletes a resource organization by ID.
    /// </summary>
    /// <param name="id">The ID of the resource organization to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource organizations.
    /// </summary>
    /// <param name="items">The list of resource organizations to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceOrg> items);

    /// <summary>
    /// Updates the sort order of resource organizations without resetting IDs.
    /// </summary>
    /// <param name="items">The list of resource organizations with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceOrg> items);
}

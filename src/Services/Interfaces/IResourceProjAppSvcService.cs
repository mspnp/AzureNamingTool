using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource project/application/services.
/// </summary>
public interface IResourceProjAppSvcService
{
    /// <summary>
    /// Retrieves all resource project/application/services.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource project/application/service by ID.
    /// </summary>
    /// <param name="id">The ID of the resource project/application/service.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource project/application/service.
    /// </summary>
    /// <param name="item">The resource project/application/service to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceProjAppSvc item);

    /// <summary>
    /// Deletes a resource project/application/service by ID.
    /// </summary>
    /// <param name="id">The ID of the resource project/application/service to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource project/application/services.
    /// </summary>
    /// <param name="items">The list of resource project/application/services to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceProjAppSvc> items);

    /// <summary>
    /// Updates the sort order of resource project/application/services without resetting IDs.
    /// </summary>
    /// <param name="items">The list of resource project/application/services with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceProjAppSvc> items);
}

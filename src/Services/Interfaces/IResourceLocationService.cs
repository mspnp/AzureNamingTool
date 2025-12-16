using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource locations.
/// </summary>
public interface IResourceLocationService
{
    /// <summary>
    /// Retrieves all resource locations.
    /// </summary>
    /// <param name="admin">Flag indicating whether to include all locations or only enabled ones.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource location by ID.
    /// </summary>
    /// <param name="id">The ID of the resource location.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource location.
    /// </summary>
    /// <param name="item">The resource location to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceLocation item);

    /// <summary>
    /// Deletes a resource location by ID.
    /// </summary>
    /// <param name="id">The ID of the resource location to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource locations.
    /// </summary>
    /// <param name="items">The list of resource locations to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceLocation> items);

    /// <summary>
    /// Refreshes resource locations from Azure data.
    /// </summary>
    /// <param name="shortNameReset">Flag indicating whether to reset short names.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> RefreshResourceLocationsAsync(bool shortNameReset = false);
}

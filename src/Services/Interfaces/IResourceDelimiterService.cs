using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource delimiters.
/// </summary>
public interface IResourceDelimiterService
{
    /// <summary>
    /// Retrieves all resource delimiters.
    /// </summary>
    /// <param name="admin">Flag indicating whether to include all delimiters or only enabled ones.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin);

    /// <summary>
    /// Retrieves a specific resource delimiter by ID.
    /// </summary>
    /// <param name="id">The ID of the resource delimiter.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Retrieves the current active resource delimiter.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetCurrentItemAsync();

    /// <summary>
    /// Posts a new resource delimiter.
    /// </summary>
    /// <param name="item">The resource delimiter to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceDelimiter item);

    /// <summary>
    /// Posts a batch configuration of resource delimiters.
    /// </summary>
    /// <param name="items">The list of resource delimiters to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceDelimiter> items);
}

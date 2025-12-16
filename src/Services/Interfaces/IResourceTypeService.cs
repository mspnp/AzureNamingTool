using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource types.
/// </summary>
public interface IResourceTypeService
{
    /// <summary>
    /// Retrieves all resource types.
    /// </summary>
    /// <param name="admin">Flag indicating whether to include all resource types or only enabled ones.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource type by ID.
    /// </summary>
    /// <param name="id">The ID of the resource type.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource type.
    /// </summary>
    /// <param name="item">The resource type to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceType item);

    /// <summary>
    /// Deletes a resource type by ID.
    /// </summary>
    /// <param name="id">The ID of the resource type to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource types.
    /// </summary>
    /// <param name="items">The list of resource types to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceType> items);

    /// <summary>
    /// Gets type categories from a list of resource types.
    /// </summary>
    /// <param name="types">The list of resource types.</param>
    /// <returns>A list of distinct type categories.</returns>
    List<string> GetTypeCategories(List<ResourceType> types);

    /// <summary>
    /// Gets filtered resource types based on a filter string.
    /// </summary>
    /// <param name="types">The list of resource types to filter.</param>
    /// <param name="filter">The filter string.</param>
    /// <returns>A filtered list of resource types.</returns>
    List<ResourceType> GetFilteredResourceTypes(List<ResourceType> types, string filter);

    /// <summary>
    /// Refreshes resource types from Azure data.
    /// </summary>
    /// <param name="shortNameReset">Flag indicating whether to reset short names.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> RefreshResourceTypesAsync(bool shortNameReset = false);

    /// <summary>
    /// Updates type components based on an operation.
    /// </summary>
    /// <param name="operation">The operation to perform.</param>
    /// <param name="componentid">The component ID.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateTypeComponentsAsync(string operation, int componentid);

    /// <summary>
    /// Validates a resource type name.
    /// </summary>
    /// <param name="validateNameRequest">The validation request.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> ValidateResourceTypeNameAsync(ValidateNameRequest validateNameRequest);
}

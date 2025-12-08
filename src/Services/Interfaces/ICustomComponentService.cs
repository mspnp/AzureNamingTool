using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing custom components.
/// </summary>
public interface ICustomComponentService
{
    /// <summary>
    /// Retrieves all custom components.
    /// </summary>
    /// <param name="admin">Flag indicating whether to include all components or only enabled ones.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific custom component by ID.
    /// </summary>
    /// <param name="id">The ID of the custom component.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new custom component.
    /// </summary>
    /// <param name="item">The custom component to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(CustomComponent item);

    /// <summary>
    /// Deletes a custom component by ID.
    /// </summary>
    /// <param name="id">The ID of the custom component to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of custom components.
    /// </summary>
    /// <param name="items">The list of custom components to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<CustomComponent> items);

    /// <summary>
    /// Retrieves custom components by parent component ID.
    /// </summary>
    /// <param name="parentcomponetid">The parent component ID.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsByParentComponentIdAsync(int parentcomponetid);

    /// <summary>
    /// Retrieves custom components by parent type.
    /// </summary>
    /// <param name="parenttype">The parent type.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsByParentTypeAsync(string parenttype);

    /// <summary>
    /// Deletes custom components by parent component ID.
    /// </summary>
    /// <param name="parentcomponentid">The parent component ID.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteByParentComponentIdAsync(int parentcomponentid);

    /// <summary>
    /// Updates the sort order of custom components without resetting IDs.
    /// </summary>
    /// <param name="items">List of custom components with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<CustomComponent> items);
}

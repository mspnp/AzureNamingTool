using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource unit/departments.
/// </summary>
public interface IResourceUnitDeptService
{
    /// <summary>
    /// Retrieves all resource unit/departments.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific resource unit/department by ID.
    /// </summary>
    /// <param name="id">The ID of the resource unit/department.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new resource unit/department.
    /// </summary>
    /// <param name="item">The resource unit/department to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(ResourceUnitDept item);

    /// <summary>
    /// Deletes a resource unit/department by ID.
    /// </summary>
    /// <param name="id">The ID of the resource unit/department to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of resource unit/departments.
    /// </summary>
    /// <param name="items">The list of resource unit/departments to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<ResourceUnitDept> items);

    /// <summary>
    /// Updates the sort order of resource unit/departments without resetting IDs.
    /// </summary>
    /// <param name="items">The list of resource unit/departments with updated sort orders.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceUnitDept> items);
}

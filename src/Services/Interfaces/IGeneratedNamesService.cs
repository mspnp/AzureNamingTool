using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing generated resource names.
/// </summary>
public interface IGeneratedNamesService
{
    /// <summary>
    /// Retrieves all generated names.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific generated name by ID.
    /// </summary>
    /// <param name="id">The ID of the generated name.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new generated name.
    /// </summary>
    /// <param name="generatedName">The generated name to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(GeneratedName generatedName);

    /// <summary>
    /// Deletes a generated name by ID.
    /// </summary>
    /// <param name="id">The ID of the generated name to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Deletes all generated names.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteAllItemsAsync();

    /// <summary>
    /// Posts a batch configuration of generated names.
    /// </summary>
    /// <param name="items">The list of generated names to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<GeneratedName> items);
}

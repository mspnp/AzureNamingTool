using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing administrative log messages.
/// </summary>
public interface IAdminLogService
{
    /// <summary>
    /// Retrieves all administrative log messages.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync();

    /// <summary>
    /// Retrieves a specific administrative log message by ID.
    /// </summary>
    /// <param name="id">The ID of the log message.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(int id);

    /// <summary>
    /// Posts a new administrative log message.
    /// </summary>
    /// <param name="message">The log message to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(AdminLogMessage message);

    /// <summary>
    /// Deletes an administrative log message by ID.
    /// </summary>
    /// <param name="id">The ID of the log message to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Deletes all administrative log messages.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteAllItemsAsync();

    /// <summary>
    /// Posts a batch configuration of administrative log messages.
    /// </summary>
    /// <param name="items">The list of log messages to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<AdminLogMessage> items);
}

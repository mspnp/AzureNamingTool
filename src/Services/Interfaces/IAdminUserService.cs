using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing administrative users.
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Retrieves all administrative users.
    /// </summary>
    /// <param name="admin">Whether to filter by admin status.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemsAsync(bool admin = true);

    /// <summary>
    /// Retrieves a specific administrative user by name.
    /// </summary>
    /// <param name="name">The name of the user.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetItemAsync(string name);

    /// <summary>
    /// Posts a new administrative user.
    /// </summary>
    /// <param name="item">The user to post.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostItemAsync(AdminUser item);

    /// <summary>
    /// Deletes an administrative user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> DeleteItemAsync(int id);

    /// <summary>
    /// Posts a batch configuration of administrative users.
    /// </summary>
    /// <param name="items">The list of users to configure.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(List<AdminUser> items);
}

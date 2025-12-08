using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for administrative operations including password and API key management.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Updates the administrator password.
    /// </summary>
    /// <param name="password">The new password.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdatePasswordAsync(string password);

    /// <summary>
    /// Generates a new API key.
    /// </summary>
    /// <param name="type">The type of API key to generate.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GenerateAPIKeyAsync(string type);

    /// <summary>
    /// Updates an existing API key.
    /// </summary>
    /// <param name="apikey">The new API key value.</param>
    /// <param name="type">The type of API key to update.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateAPIKeyAsync(string apikey, string type);

    /// <summary>
    /// Updates the identity header name.
    /// </summary>
    /// <param name="identityheadername">The new identity header name.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> UpdateIdentityHeaderNameAsync(string identityheadername);
}

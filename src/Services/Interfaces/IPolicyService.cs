using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for Azure Policy generation operations.
/// </summary>
public interface IPolicyService
{
    /// <summary>
    /// Gets the Azure Policy definition based on current configuration.
    /// </summary>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> GetPolicyAsync();
}

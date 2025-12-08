using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for managing resource naming requests.
/// </summary>
public interface IResourceNamingRequestService
{
    /// <summary>
    /// Requests a resource name with explicit component values.
    /// </summary>
    /// <param name="request">The resource naming request with components.</param>
    /// <returns>A <see cref="Task{ResourceNameResponse}"/> representing the asynchronous operation.</returns>
    Task<ResourceNameResponse> RequestNameWithComponentsAsync(ResourceNameRequestWithComponents request);

    /// <summary>
    /// Requests a resource name based on request parameters.
    /// </summary>
    /// <param name="request">The resource naming request.</param>
    /// <returns>A <see cref="Task{ResourceNameResponse}"/> representing the asynchronous operation.</returns>
    Task<ResourceNameResponse> RequestNameAsync(ResourceNameRequest request);
}

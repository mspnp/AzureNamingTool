using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Coordinates operations between ResourceComponent and ResourceType services
    /// to avoid circular dependencies while maintaining business logic integrity.
    /// </summary>
    public interface IResourceConfigurationCoordinator
    {
        /// <summary>
        /// Updates resource types when a component is deleted.
        /// Removes the component from Optional and Exclude lists in all resource types.
        /// </summary>
        /// <param name="componentName">The name of the component to remove from resource types.</param>
        Task UpdateTypesOnComponentDeleteAsync(string componentName);

        /// <summary>
        /// Gets a resource component by ID for resource type validation.
        /// </summary>
        /// <param name="componentId">The ID of the component to retrieve.</param>
        /// <returns>Service response containing the component if found.</returns>
        Task<ServiceResponse> GetComponentForTypeValidationAsync(int componentId);

        /// <summary>
        /// Deletes all custom components associated with a parent component.
        /// </summary>
        /// <param name="parentComponentId">The ID of the parent component.</param>
        Task DeleteCustomComponentsByParentIdAsync(int parentComponentId);
    }
}

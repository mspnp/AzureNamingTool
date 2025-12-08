#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Coordinates operations between ResourceComponent and ResourceType services
    /// to avoid circular dependencies.
    /// </summary>
    public class ResourceConfigurationCoordinator : IResourceConfigurationCoordinator
    {
        private readonly IConfigurationRepository<ResourceType> _resourceTypeRepository;
        private readonly IConfigurationRepository<ResourceComponent> _resourceComponentRepository;
        private readonly IConfigurationRepository<CustomComponent> _customComponentRepository;
        private readonly IAdminLogService _adminLogService;

        public ResourceConfigurationCoordinator(
            IConfigurationRepository<ResourceType> resourceTypeRepository,
            IConfigurationRepository<ResourceComponent> resourceComponentRepository,
            IConfigurationRepository<CustomComponent> customComponentRepository,
            IAdminLogService adminLogService)
        {
            _resourceTypeRepository = resourceTypeRepository;
            _resourceComponentRepository = resourceComponentRepository;
            _customComponentRepository = customComponentRepository;
            _adminLogService = adminLogService;
        }

        /// <inheritdoc/>
        public async Task UpdateTypesOnComponentDeleteAsync(string componentName)
        {
            try
            {
                var resourceTypes = await _resourceTypeRepository.GetAllAsync();
                if (resourceTypes == null || !resourceTypes.Any())
                {
                    return;
                }

                bool updated = false;
                foreach (var resourceType in resourceTypes)
                {
                    // Remove from Optional list
                    var optionalValues = new List<string>(resourceType.Optional.Split(','));
                    if (optionalValues.Contains(GeneralHelper.NormalizeName(componentName, false)))
                    {
                        optionalValues.Remove(GeneralHelper.NormalizeName(componentName, false));
                        resourceType.Optional = string.Join(",", optionalValues.Where(v => !string.IsNullOrEmpty(v)));
                        updated = true;
                    }

                    // Remove from Exclude list
                    var excludeValues = new List<string>(resourceType.Exclude.Split(','));
                    if (excludeValues.Contains(GeneralHelper.NormalizeName(componentName, false)))
                    {
                        excludeValues.Remove(GeneralHelper.NormalizeName(componentName, false));
                        resourceType.Exclude = string.Join(",", excludeValues.Where(v => !string.IsNullOrEmpty(v)));
                        updated = true;
                    }

                    if (updated)
                    {
                        await _resourceTypeRepository.SaveAsync(resourceType);
                        updated = false; // Reset for next type
                    }
                }

                await _adminLogService.PostItemAsync(new AdminLogMessage
                {
                    Title = "SUCCESS",
                    Message = $"Updated resource types after component '{componentName}' deletion."
                });
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage
                {
                    Title = "ERROR",
                    Message = ex.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse> GetComponentForTypeValidationAsync(int componentId)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                var component = await _resourceComponentRepository.GetByIdAsync(componentId);
                if (component != null)
                {
                    serviceResponse.ResponseObject = component;
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "Component not found!";
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage
                {
                    Title = "ERROR",
                    Message = ex.Message
                });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <inheritdoc/>
        public async Task DeleteCustomComponentsByParentIdAsync(int parentComponentId)
        {
            try
            {
                // Get the parent component
                var parentComponent = await _resourceComponentRepository.GetByIdAsync(parentComponentId);
                if (parentComponent == null)
                {
                    return;
                }

                string parentComponentName = GeneralHelper.NormalizeName(parentComponent.Name, true);

                // Get all custom components
                var customComponents = (await _customComponentRepository.GetAllAsync()).ToList();
                if (customComponents == null || !customComponents.Any())
                {
                    return;
                }

                // Filter and delete custom components for this parent
                var toDelete = customComponents.Where(x => x.ParentComponent == parentComponentName).ToList();
                foreach (var customComponent in toDelete)
                {
                    await _customComponentRepository.DeleteAsync((int)customComponent.Id);
                }

                if (toDelete.Any())
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage
                    {
                        Title = "SUCCESS",
                        Message = $"Deleted {toDelete.Count} custom component(s) for parent component '{parentComponent.Name}'."
                    });
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage
                {
                    Title = "ERROR",
                    Message = ex.Message
                });
            }
        }
    }
}

#pragma warning restore CS1591
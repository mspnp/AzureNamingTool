#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for loading services data.
    /// </summary>
    public class ServicesHelper
    {
        private readonly IResourceComponentService _resourceComponentService;
        private readonly IResourceDelimiterService _resourceDelimiterService;
        private readonly IResourceEnvironmentService _resourceEnvironmentService;
        private readonly IResourceLocationService _resourceLocationService;
        private readonly IResourceOrgService _resourceOrgService;
        private readonly IResourceProjAppSvcService _resourceProjAppSvcService;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IResourceUnitDeptService _resourceUnitDeptService;
        private readonly IResourceFunctionService _resourceFunctionService;
        private readonly ICustomComponentService _customComponentService;
        private readonly IGeneratedNamesService _generatedNamesService;
        private readonly IAdminLogService _adminLogService;
        private readonly IAdminUserService _adminUserService;

        public ServicesHelper(
            IResourceComponentService resourceComponentService,
            IResourceDelimiterService resourceDelimiterService,
            IResourceEnvironmentService resourceEnvironmentService,
            IResourceLocationService resourceLocationService,
            IResourceOrgService resourceOrgService,
            IResourceProjAppSvcService resourceProjAppSvcService,
            IResourceTypeService resourceTypeService,
            IResourceUnitDeptService resourceUnitDeptService,
            IResourceFunctionService resourceFunctionService,
            ICustomComponentService customComponentService,
            IGeneratedNamesService generatedNamesService,
            IAdminLogService adminLogService,
            IAdminUserService adminUserService)
        {
            _resourceComponentService = resourceComponentService;
            _resourceDelimiterService = resourceDelimiterService;
            _resourceEnvironmentService = resourceEnvironmentService;
            _resourceLocationService = resourceLocationService;
            _resourceOrgService = resourceOrgService;
            _resourceProjAppSvcService = resourceProjAppSvcService;
            _resourceTypeService = resourceTypeService;
            _resourceUnitDeptService = resourceUnitDeptService;
            _resourceFunctionService = resourceFunctionService;
            _customComponentService = customComponentService;
            _generatedNamesService = generatedNamesService;
            _adminLogService = adminLogService;
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// Loads the services data.
        /// </summary>
        /// <param name="servicesData">The services data.</param>
        /// <param name="admin">A flag indicating whether the user is an admin.</param>
        /// <returns>The loaded services data.</returns>
        public async Task<ServicesData> LoadServicesData(ServicesData servicesData, bool admin)
        {
            ServiceResponse serviceResponse;
            try
            {
                serviceResponse = await _resourceComponentService.GetItemsAsync(admin);
                servicesData.ResourceComponents = (List<ResourceComponent>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceDelimiterService.GetItemsAsync(admin);
                servicesData.ResourceDelimiters = (List<ResourceDelimiter>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceEnvironmentService.GetItemsAsync();
                servicesData.ResourceEnvironments = (List<ResourceEnvironment>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceLocationService.GetItemsAsync(admin);
                servicesData.ResourceLocations = (List<ResourceLocation>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceOrgService.GetItemsAsync();
                servicesData.ResourceOrgs = (List<ResourceOrg>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceProjAppSvcService.GetItemsAsync();
                servicesData.ResourceProjAppSvcs = (List<ResourceProjAppSvc>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceTypeService.GetItemsAsync(admin);
                servicesData.ResourceTypes = (List<ResourceType>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceUnitDeptService.GetItemsAsync();
                servicesData.ResourceUnitDepts = (List<ResourceUnitDept>?)serviceResponse.ResponseObject;
                serviceResponse = await _resourceFunctionService.GetItemsAsync();
                servicesData.ResourceFunctions = (List<ResourceFunction>?)serviceResponse.ResponseObject;
                serviceResponse = await _customComponentService.GetItemsAsync();
                servicesData.CustomComponents = (List<CustomComponent>?)serviceResponse.ResponseObject;
                serviceResponse = await _generatedNamesService.GetItemsAsync();
                servicesData.GeneratedNames = (List<GeneratedName>?)serviceResponse.ResponseObject;
                serviceResponse = await _adminLogService.GetItemsAsync();
                servicesData.AdminLogMessages = (List<AdminLogMessage>?)serviceResponse.ResponseObject;
                serviceResponse = await _adminUserService.GetItemsAsync();
                servicesData.AdminUsers = (List<AdminUser>?)serviceResponse.ResponseObject;
                return servicesData;
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return servicesData;
            }
        }
    }
}

#pragma warning restore CS1591
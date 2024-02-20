using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for loading services data.
    /// </summary>
    public class ServicesHelper
    {
        /// <summary>
        /// Loads the services data.
        /// </summary>
        /// <param name="servicesData">The services data.</param>
        /// <param name="admin">A flag indicating whether the user is an admin.</param>
        /// <returns>The loaded services data.</returns>
        public static async Task<ServicesData> LoadServicesData(ServicesData servicesData, bool admin)
        {
            ServiceResponse serviceResponse;
            try
            {
                serviceResponse = await ResourceComponentService.GetItems(admin);
                servicesData.ResourceComponents = (List<ResourceComponent>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceDelimiterService.GetItems(admin);
                servicesData.ResourceDelimiters = (List<ResourceDelimiter>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceEnvironmentService.GetItems();
                servicesData.ResourceEnvironments = (List<ResourceEnvironment>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceLocationService.GetItems(admin);
                servicesData.ResourceLocations = (List<ResourceLocation>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceOrgService.GetItems();
                servicesData.ResourceOrgs = (List<ResourceOrg>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceProjAppSvcService.GetItems();
                servicesData.ResourceProjAppSvcs = (List<ResourceProjAppSvc>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceTypeService.GetItems(admin);
                servicesData.ResourceTypes = (List<ResourceType>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceUnitDeptService.GetItems();
                servicesData.ResourceUnitDepts = (List<ResourceUnitDept>?)serviceResponse.ResponseObject;
                serviceResponse = await ResourceFunctionService.GetItems();
                servicesData.ResourceFunctions = (List<ResourceFunction>?)serviceResponse.ResponseObject;
                serviceResponse = await CustomComponentService.GetItems();
                servicesData.CustomComponents = (List<CustomComponent>?)serviceResponse.ResponseObject;
                serviceResponse = await GeneratedNamesService.GetItems();
                servicesData.GeneratedNames = (List<GeneratedName>?)serviceResponse.ResponseObject;
                serviceResponse = await AdminLogService.GetItems();
                servicesData.AdminLogMessages = (List<AdminLogMessage>?)serviceResponse.ResponseObject;
                serviceResponse = await AdminUserService.GetItems();
                servicesData.AdminUsers = (List<AdminUser>?)serviceResponse.ResponseObject;
                return servicesData;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return servicesData;
            }
        }
    }
}

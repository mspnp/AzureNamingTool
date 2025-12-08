#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the import and export of configuration data.
    /// </summary>
    public class ImportExportService : IImportExportService
    {
        private readonly IResourceComponentService _resourceComponentService;
        private readonly IResourceDelimiterService _resourceDelimiterService;
        private readonly IResourceEnvironmentService _resourceEnvironmentService;
        private readonly IResourceFunctionService _resourceFunctionService;
        private readonly IResourceLocationService _resourceLocationService;
        private readonly IResourceOrgService _resourceOrgService;
        private readonly IResourceProjAppSvcService _resourceProjAppSvcService;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IResourceUnitDeptService _resourceUnitDeptService;
        private readonly ICustomComponentService _customComponentService;
        private readonly IGeneratedNamesService _generatedNamesService;
        private readonly IAdminLogService _adminLogService;
        private readonly IAdminUserService _adminUserService;

        public ImportExportService(
            IResourceComponentService resourceComponentService,
            IResourceDelimiterService resourceDelimiterService,
            IResourceEnvironmentService resourceEnvironmentService,
            IResourceFunctionService resourceFunctionService,
            IResourceLocationService resourceLocationService,
            IResourceOrgService resourceOrgService,
            IResourceProjAppSvcService resourceProjAppSvcService,
            IResourceTypeService resourceTypeService,
            IResourceUnitDeptService resourceUnitDeptService,
            ICustomComponentService customComponentService,
            IGeneratedNamesService generatedNamesService,
            IAdminLogService adminLogService,
            IAdminUserService adminUserService)
        {
            _resourceComponentService = resourceComponentService;
            _resourceDelimiterService = resourceDelimiterService;
            _resourceEnvironmentService = resourceEnvironmentService;
            _resourceFunctionService = resourceFunctionService;
            _resourceLocationService = resourceLocationService;
            _resourceOrgService = resourceOrgService;
            _resourceProjAppSvcService = resourceProjAppSvcService;
            _resourceTypeService = resourceTypeService;
            _resourceUnitDeptService = resourceUnitDeptService;
            _customComponentService = customComponentService;
            _generatedNamesService = generatedNamesService;
            _adminLogService = adminLogService;
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// Export the configuration data.
        /// </summary>
        /// <param name="includeadmin">Flag to include admin settings in the export.</param>
        /// <returns>The service response containing the exported configuration data.</returns>
        public async Task<ServiceResponse> ExportConfigAsync(bool includeadmin = false)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                ConfigurationData configdata = new();
                // Get the current data
                //ResourceComponents
                serviceResponse = await _resourceComponentService.GetItemsAsync(true);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        configdata.ResourceComponents = serviceResponse.ResponseObject!;
                    }
                }

                //ResourceDelimiters
                serviceResponse = await _resourceDelimiterService.GetItemsAsync(true);
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceDelimiters = serviceResponse.ResponseObject!;
                }

                //ResourceEnvironments
                serviceResponse = await _resourceEnvironmentService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceEnvironments = serviceResponse.ResponseObject!;
                }

                // ResourceFunctions
                serviceResponse = await _resourceFunctionService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceFunctions = serviceResponse.ResponseObject!;
                }

                // ResourceLocations
                serviceResponse = await _resourceLocationService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceLocations = serviceResponse.ResponseObject!;
                }

                // ResourceOrgs
                serviceResponse = await _resourceOrgService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceOrgs = serviceResponse.ResponseObject!;
                }

                // ResourceProjAppSvc
                serviceResponse = await _resourceProjAppSvcService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceProjAppSvcs = serviceResponse.ResponseObject!;
                }

                // ResourceTypes
                serviceResponse = await _resourceTypeService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceTypes = serviceResponse.ResponseObject!;
                }

                // ResourceUnitDepts
                serviceResponse = await _resourceUnitDeptService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceUnitDepts = serviceResponse.ResponseObject!;
                }

                // CustomComponents
                serviceResponse = await _customComponentService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.CustomComponents = serviceResponse.ResponseObject!;
                }

                //GeneratedNames
                serviceResponse = await _generatedNamesService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.GeneratedNames = serviceResponse.ResponseObject!;
                }

                //AdminLogs
                serviceResponse = await _adminLogService.GetItemsAsync();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.AdminLogs = serviceResponse.ResponseObject;
                }

                // Get the current settings
                var config = ConfigurationHelper.GetConfigurationData();
                configdata.DismissedAlerts = config.DismissedAlerts;
                configdata.DuplicateNamesAllowed = config.DuplicateNamesAllowed;
                configdata.ConnectivityCheckEnabled = config.ConnectivityCheckEnabled;
                configdata.GenerationWebhook = config.GenerationWebhook;
                configdata.ResourceTypeEditingAllowed = config.ResourceTypeEditingAllowed;
                configdata.AutoIncrementResourceInstance = config.AutoIncrementResourceInstance;
                configdata.InstructionsEnabled = config.InstructionsEnabled;
                configdata.GeneratedNamesLogEnabled = config.GeneratedNamesLogEnabled;
                configdata.ConfigurationEnabled = config.ConfigurationEnabled;
                configdata.ReferenceEnabled = config.ReferenceEnabled;
                configdata.LatestNewsEnabled = config.LatestNewsEnabled;
                configdata.RetainGenerateSelections = config.RetainGenerateSelections;
                configdata.CustomHomeContent = config.CustomHomeContent;
                configdata.CustomLogoPath = config.CustomLogoPath;
                configdata.CustomToolName = config.CustomToolName;
                configdata.ShowAdminDetailsToAllUsers = config.ShowAdminDetailsToAllUsers;

                // Get the security settings
                if (includeadmin)
                {
                    configdata.SALTKey = config.SALTKey;
                    configdata.AdminPassword = config.AdminPassword;
                    configdata.APIKey = config.APIKey;
                    configdata.ReadOnlyAPIKey = config.ReadOnlyAPIKey;
                    configdata.NameGenerationAPIKey = config.NameGenerationAPIKey;
                    //IdentityHeaderName
                    configdata.IdentityHeaderName = config.IdentityHeaderName;
                    //AdminUsers
                    serviceResponse = await _adminUserService.GetItemsAsync();
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        configdata.AdminUsers = serviceResponse.ResponseObject!;
                    }
                }

                serviceResponse.ResponseObject = configdata;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Import the configuration data.
        /// </summary>
        /// <param name="configdata">The configuration data to import.</param>
        /// <returns>The service response indicating the success or failure of the import.</returns>
        public async Task<ServiceResponse> PostConfigAsync(ConfigurationData configdata)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Write all the configurations
                await _resourceComponentService.PostConfigAsync(configdata.ResourceComponents);
                await _resourceDelimiterService.PostConfigAsync(configdata.ResourceDelimiters);
                await _resourceEnvironmentService.PostConfigAsync(configdata.ResourceEnvironments);
                await _resourceFunctionService.PostConfigAsync(configdata.ResourceFunctions);
                await _resourceLocationService.PostConfigAsync(configdata.ResourceLocations);
                await _resourceOrgService.PostConfigAsync(configdata.ResourceOrgs);
                await _resourceProjAppSvcService.PostConfigAsync(configdata.ResourceProjAppSvcs);
                await _resourceTypeService.PostConfigAsync(configdata.ResourceTypes);
                await _resourceUnitDeptService.PostConfigAsync(configdata.ResourceUnitDepts);
                await _customComponentService.PostConfigAsync(configdata.CustomComponents);
                await _generatedNamesService.PostConfigAsync(configdata.GeneratedNames);
                if (GeneralHelper.IsNotNull(configdata.AdminUsers))
                {
                    await _adminUserService.PostConfigAsync(configdata.AdminUsers);
                }
                if (GeneralHelper.IsNotNull(configdata.AdminLogs))
                {
                    await _adminLogService.PostConfigAsync(configdata.AdminLogs);
                }

                var config = ConfigurationHelper.GetConfigurationData();
                config.DismissedAlerts = configdata.DismissedAlerts;
                config.DuplicateNamesAllowed = configdata.DuplicateNamesAllowed;
                config.ConnectivityCheckEnabled = configdata.ConnectivityCheckEnabled;

                // Set the admin settings, if they are included in the import
                if (GeneralHelper.IsNotNull(configdata.SALTKey))
                {
                    config.SALTKey = configdata.SALTKey;
                }
                if (GeneralHelper.IsNotNull(configdata.AdminPassword))
                {
                    config.AdminPassword = configdata.AdminPassword;
                }
                if (GeneralHelper.IsNotNull(configdata.APIKey))
                {
                    config.APIKey = configdata.APIKey;
                }
                if (GeneralHelper.IsNotNull(configdata.ReadOnlyAPIKey))
                {
                    config.ReadOnlyAPIKey = configdata.ReadOnlyAPIKey;
                }
                if (GeneralHelper.IsNotNull(configdata.NameGenerationAPIKey))
                {
                    config.NameGenerationAPIKey = configdata.NameGenerationAPIKey;
                }
                if (GeneralHelper.IsNotNull(configdata.IdentityHeaderName))
                {
                    config.IdentityHeaderName = configdata.IdentityHeaderName;
                }
                if (GeneralHelper.IsNotNull(configdata.ResourceTypeEditingAllowed))
                {
                    config.ResourceTypeEditingAllowed = configdata.ResourceTypeEditingAllowed;
                }
                if (GeneralHelper.IsNotNull(configdata.AutoIncrementResourceInstance))
                {
                    config.AutoIncrementResourceInstance = configdata.AutoIncrementResourceInstance;
                }
                if (GeneralHelper.IsNotNull(configdata.InstructionsEnabled))
                {
                    config.InstructionsEnabled = configdata.InstructionsEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.GeneratedNamesLogEnabled))
                {
                    config.GeneratedNamesLogEnabled = configdata.GeneratedNamesLogEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.ConfigurationEnabled))
                {
                    config.ConfigurationEnabled = configdata.ConfigurationEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.ReferenceEnabled))
                {
                    config.ReferenceEnabled = configdata.ReferenceEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.LatestNewsEnabled))
                {
                    config.LatestNewsEnabled = configdata.LatestNewsEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.RetainGenerateSelections))
                {
                    config.RetainGenerateSelections = configdata.RetainGenerateSelections;
                }
                if (GeneralHelper.IsNotNull(configdata.CustomHomeContent))
                {
                    config.CustomHomeContent = configdata.CustomHomeContent;
                }
                if (GeneralHelper.IsNotNull(configdata.CustomLogoPath))
                {
                    config.CustomLogoPath = configdata.CustomLogoPath;
                }
                if (GeneralHelper.IsNotNull(configdata.CustomToolName))
                {
                    config.CustomToolName = configdata.CustomToolName;
                }
                if (GeneralHelper.IsNotNull(configdata.ShowAdminDetailsToAllUsers))
                {
                    config.ShowAdminDetailsToAllUsers = configdata.ShowAdminDetailsToAllUsers;
                }
                var jsonWriteOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

                var newJson = JsonSerializer.Serialize(config, jsonWriteOptions);

                var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/appsettings.json");
                File.WriteAllText(appSettingsPath, newJson);
                CacheHelper.ClearAllCache();
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}

#pragma warning restore CS1591
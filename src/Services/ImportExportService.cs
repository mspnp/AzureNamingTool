using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the import and export of configuration data.
    /// </summary>
    public class ImportExportService
    {
        /// <summary>
        /// Export the configuration data.
        /// </summary>
        /// <param name="includeadmin">Flag to include admin settings in the export.</param>
        /// <returns>The service response containing the exported configuration data.</returns>
        public static async Task<ServiceResponse> ExportConfig(bool includeadmin = false)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                ConfigurationData configdata = new();
                // Get the current data
                //ResourceComponents
                serviceResponse = await ResourceComponentService.GetItems(true);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        configdata.ResourceComponents = serviceResponse.ResponseObject!;
                    }
                }

                //ResourceDelimiters
                serviceResponse = await ResourceDelimiterService.GetItems(true);
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceDelimiters = serviceResponse.ResponseObject!;
                }

                //ResourceEnvironments
                serviceResponse = await ResourceEnvironmentService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceEnvironments = serviceResponse.ResponseObject!;
                }

                // ResourceFunctions
                serviceResponse = await ResourceFunctionService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceFunctions = serviceResponse.ResponseObject!;
                }

                // ResourceLocations
                serviceResponse = await ResourceLocationService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceLocations = serviceResponse.ResponseObject!;
                }

                // ResourceOrgs
                serviceResponse = await ResourceOrgService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceOrgs = serviceResponse.ResponseObject!;
                }

                // ResourceProjAppSvc
                serviceResponse = await ResourceProjAppSvcService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceProjAppSvcs = serviceResponse.ResponseObject!;
                }

                // ResourceTypes
                serviceResponse = await ResourceTypeService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceTypes = serviceResponse.ResponseObject!;
                }

                // ResourceUnitDepts
                serviceResponse = await ResourceUnitDeptService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.ResourceUnitDepts = serviceResponse.ResponseObject!;
                }

                // CustomComponents
                serviceResponse = await CustomComponentService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.CustomComponents = serviceResponse.ResponseObject!;
                }

                //GeneratedNames
                serviceResponse = await GeneratedNamesService.GetItems();
                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                {
                    configdata.GeneratedNames = serviceResponse.ResponseObject!;
                }

                //AdminLogs
                serviceResponse = await AdminLogService.GetItems();
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
                configdata.LatestNewsEnabled = config.LatestNewsEnabled;
                configdata.RetainGenerateSelections = config.RetainGenerateSelections;

                // Get the security settings
                if (includeadmin)
                {
                    configdata.SALTKey = config.SALTKey;
                    configdata.AdminPassword = config.AdminPassword;
                    configdata.APIKey = config.APIKey;
                    configdata.ReadOnlyAPIKey = config.ReadOnlyAPIKey;
                    //IdentityHeaderName
                    configdata.IdentityHeaderName = config.IdentityHeaderName;
                    //AdminUsers
                    serviceResponse = await AdminUserService.GetItems();
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        public static async Task<ServiceResponse> PostConfig(ConfigurationData configdata)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Write all the configurations
                await ResourceComponentService.PostConfig(configdata.ResourceComponents);
                await ResourceDelimiterService.PostConfig(configdata.ResourceDelimiters);
                await ResourceEnvironmentService.PostConfig(configdata.ResourceEnvironments);
                await ResourceFunctionService.PostConfig(configdata.ResourceFunctions);
                await ResourceLocationService.PostConfig(configdata.ResourceLocations);
                await ResourceOrgService.PostConfig(configdata.ResourceOrgs);
                await ResourceProjAppSvcService.PostConfig(configdata.ResourceProjAppSvcs);
                await ResourceTypeService.PostConfig(configdata.ResourceTypes);
                await ResourceUnitDeptService.PostConfig(configdata.ResourceUnitDepts);
                await CustomComponentService.PostConfig(configdata.CustomComponents);
                await GeneratedNamesService.PostConfig(configdata.GeneratedNames);
                if (GeneralHelper.IsNotNull(configdata.AdminUsers))
                {
                    await AdminUserService.PostConfig(configdata.AdminUsers);
                }
                if (GeneralHelper.IsNotNull(configdata.AdminLogs))
                {
                    await AdminLogService.PostConfig(configdata.AdminLogs);
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
                if (GeneralHelper.IsNotNull(configdata.LatestNewsEnabled))
                {
                    config.LatestNewsEnabled = configdata.LatestNewsEnabled;
                }
                if (GeneralHelper.IsNotNull(configdata.RetainGenerateSelections))
                {
                    config.RetainGenerateSelections = configdata.RetainGenerateSelections;
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}

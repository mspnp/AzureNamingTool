using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Services
{
    public class ImportExportService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> ExportConfig(bool includeadmin = false)
        {
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

                // Get the security settings
                if (includeadmin)
                {
                    configdata.SALTKey = config.SALTKey;
                    configdata.AdminPassword = config.AdminPassword;
                    configdata.APIKey = config.APIKey;
                    //IdentityHeaderName
                    configdata.IdentityHeaderName = config.IdentityHeaderName;
                    //AdminUsers
                    serviceResponse = await AdminUserService.GetItems();
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        configdata.AdminUsers = serviceResponse.ResponseObject!;
                    }
                    // ResourceTypeEditing
                    configdata.ResourceTypeEditingAllowed = config.ResourceTypeEditingAllowed;
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

        public static async Task<ServiceResponse> PostConfig(ConfigurationData configdata)
        {
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
                if (configdata.SALTKey != null)
                {
                    config.SALTKey = configdata.SALTKey;
                }
                if (configdata.AdminPassword != null)
                {
                    config.AdminPassword = configdata.AdminPassword;
                }
                if (configdata.APIKey != null)
                {
                    config.APIKey = configdata.APIKey;
                }
                if (configdata.IdentityHeaderName != null)
                {
                    config.IdentityHeaderName = configdata.IdentityHeaderName;
                }
                if (configdata.ResourceTypeEditingAllowed != null)
                {
                    config.ResourceTypeEditingAllowed = configdata.ResourceTypeEditingAllowed;
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

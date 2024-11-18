using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing policies.
    /// </summary>
    public class PolicyService
    {
        /// <summary>
        /// Retrieves the policy.
        /// </summary>
        /// <returns>The service response.</returns>
        public static async Task<ServiceResponse> GetPolicy()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                var delimiter = '-';
                var nameComponents = await Helpers.ConfigurationHelper.GetList<ResourceComponent>();
                var resourceTypes = await Helpers.ConfigurationHelper.GetList<ResourceType>();
                var unitDepts = await Helpers.ConfigurationHelper.GetList<ResourceUnitDept>();
                var environments = await Helpers.ConfigurationHelper.GetList<ResourceEnvironment>();
                var locations = await Helpers.ConfigurationHelper.GetList<ResourceLocation>();
                var orgs = await Helpers.ConfigurationHelper.GetList<ResourceOrg>();
                var Functions = await Helpers.ConfigurationHelper.GetList<ResourceFunction>();
                var projectAppSvcs = await Helpers.ConfigurationHelper.GetList<ResourceProjAppSvc>();

                List<String> validations = [];
                var maxSortOrder = 0;
                if (GeneralHelper.IsNotNull(nameComponents))
                {
                    foreach (var nameComponent in nameComponents)
                    {
                        var name = (String)nameComponent.Name;
                        var isEnabled = (bool)nameComponent.Enabled;
                        var sortOrder = (int)nameComponent.SortOrder;
                        maxSortOrder = sortOrder - 1;
                        if (isEnabled)
                        {
                            switch (name)
                            {
                                case "ResourceType":
                                    if (GeneralHelper.IsNotNull(resourceTypes))
                                    {
                                        AddValidations(resourceTypes, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceUnitDept":
                                    if (GeneralHelper.IsNotNull(unitDepts))
                                    {
                                        AddValidations(unitDepts, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceEnvironment":
                                    if (GeneralHelper.IsNotNull(environments))
                                    {
                                        AddValidations(environments, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceLocation":
                                    if (GeneralHelper.IsNotNull(locations))
                                    {
                                        AddValidations(locations, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceOrgs":
                                    if (GeneralHelper.IsNotNull(orgs))
                                    {
                                        AddValidations(orgs, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceFunctions":
                                    if (GeneralHelper.IsNotNull(Functions))
                                    {
                                        AddValidations(Functions, validations, delimiter, sortOrder);
                                    }
                                    break;
                                case "ResourceProjAppSvcs":
                                    if (GeneralHelper.IsNotNull(projectAppSvcs))
                                    {
                                        AddValidations(projectAppSvcs, validations, delimiter, sortOrder);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                var property = new PolicyProperty("Name Validation", "This policy enables you to restrict the name can be specified when deploying a Azure Resource.")
                {
                    PolicyRule = PolicyRuleFactory.GetNameValidationRules(validations.Select(x => new PolicyRule(x, delimiter)).ToList(), delimiter)
                };
                PolicyDefinition definition = new(property);

                //serviceResponse.ResponseObject = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(definition.ToString())).ToArray();
                serviceResponse.ResponseObject = definition;
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
        /// Adds validations to the list based on the provided parameters.
        /// </summary>
        /// <param name="list">The dynamic list of items.</param>
        /// <param name="validations">The list of validations.</param>
        /// <param name="delimiter">The delimiter character.</param>
        /// <param name="level">The level of the validations.</param>
        private static void AddValidations(dynamic list, List<string> validations, Char delimiter, int level)
        {
            if (validations.Count == 0)
            {
                foreach (var item in list)
                {
                    if (item.ShortName.Trim() != String.Empty)
                    {
                        var key = $"{item.ShortName}{delimiter}*";
                        if (!validations.Contains(key))
                            validations.Add(key);
                    }
                }
            }
            else
            {
                foreach (var item in list)
                {
                    if (item.ShortName.Trim() != String.Empty)
                    {
                        foreach (var validation in validations.Where(x => x.Count(p => p.ToString().Contains(delimiter)) == level - 1).ToList())
                        {
                            var key = $"{validation.Replace("*", "")}{item.ShortName}{delimiter}*";
                            if (!validations.Contains(key))
                                validations.Add(key);
                        }
                    }
                }
            }
        }
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing policies.
    /// </summary>
    public class PolicyService : IPolicyService
    {
        private readonly IConfigurationRepository<ResourceComponent> _componentRepository;
        private readonly IConfigurationRepository<ResourceType> _typeRepository;
        private readonly IConfigurationRepository<ResourceUnitDept> _unitDeptRepository;
        private readonly IConfigurationRepository<ResourceEnvironment> _environmentRepository;
        private readonly IConfigurationRepository<ResourceLocation> _locationRepository;
        private readonly IConfigurationRepository<ResourceOrg> _orgRepository;
        private readonly IConfigurationRepository<ResourceFunction> _functionRepository;
        private readonly IConfigurationRepository<ResourceProjAppSvc> _projAppSvcRepository;
        private readonly IAdminLogService _adminLogService;

        public PolicyService(
            IConfigurationRepository<ResourceComponent> componentRepository,
            IConfigurationRepository<ResourceType> typeRepository,
            IConfigurationRepository<ResourceUnitDept> unitDeptRepository,
            IConfigurationRepository<ResourceEnvironment> environmentRepository,
            IConfigurationRepository<ResourceLocation> locationRepository,
            IConfigurationRepository<ResourceOrg> orgRepository,
            IConfigurationRepository<ResourceFunction> functionRepository,
            IConfigurationRepository<ResourceProjAppSvc> projAppSvcRepository,
            IAdminLogService adminLogService)
        {
            _componentRepository = componentRepository;
            _typeRepository = typeRepository;
            _unitDeptRepository = unitDeptRepository;
            _environmentRepository = environmentRepository;
            _locationRepository = locationRepository;
            _orgRepository = orgRepository;
            _functionRepository = functionRepository;
            _projAppSvcRepository = projAppSvcRepository;
            _adminLogService = adminLogService;
        }

        /// <summary>
        /// Retrieves the policy.
        /// </summary>
        /// <returns>The service response.</returns>
        public async Task<ServiceResponse> GetPolicyAsync()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                var delimiter = '-';
                var nameComponents = (await _componentRepository.GetAllAsync()).ToList();
                var resourceTypes = (await _typeRepository.GetAllAsync()).ToList();
                var unitDepts = (await _unitDeptRepository.GetAllAsync()).ToList();
                var environments = (await _environmentRepository.GetAllAsync()).ToList();
                var locations = (await _locationRepository.GetAllAsync()).ToList();
                var orgs = (await _orgRepository.GetAllAsync()).ToList();
                var Functions = (await _functionRepository.GetAllAsync()).ToList();
                var projectAppSvcs = (await _projAppSvcRepository.GetAllAsync()).ToList();

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
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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

#pragma warning restore CS1591
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AzureNamingTool.Services
{
    public class ResourceNamingRequestService
    {
        /// <summary>
        /// This function will generate a resoure type name for specifed component values. This function requires full definition for all components. It is recommended to use the ResourceNameRequest API function for name generation.   
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ResourceNameResponse - Response of name generation</returns>
        public static async Task<ResourceNameResponse> RequestNameWithComponents(ResourceNameRequestWithComponents request)
        {
            ServiceResponse serviceResponse = new();
            ResourceNameResponse response = new()
            {
                Success = false
            };

            try
            {
                bool valid = true;
                bool ignoredelimeter = false;
                List<string[]> lstComponents = new();

                // Get the specified resource type
                //var resourceTypes = await ConfigurationHelper.GetList<ResourceType>();
                //var resourceType = resourceTypes.Find(x => x.Id == request.ResourceType);
                var resourceType = request.ResourceType;

                // Check static value
                if (!String.IsNullOrEmpty(resourceType.StaticValues))
                {
                    // Return the static value and message and stop generation.
                    response.ResourceName = resourceType.StaticValues;
                    response.Message = "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                    response.Success = true;
                    return response;
                }

                // Get the components
                ServiceResponse serviceresponse = new();
                serviceresponse = await ResourceComponentService.GetItems(false);
                var currentResourceComponents = serviceresponse.ResponseObject;
                dynamic d = request;

                string name = "";

                StringBuilder sbMessage = new();

                // Loop through each component
                if (GeneralHelper.IsNotNull(currentResourceComponents))
                {
                    foreach (var component in currentResourceComponents!)
                    {
                        string normalizedcomponentname = GeneralHelper.NormalizeName(component.Name, true);
                        // Check if the component is excluded for the Resource Type
                        if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                        {
                            // Attempt to retrieve value from JSON body
                            var prop = GeneralHelper.GetPropertyValue(d, component.Name);
                            string value = String.Empty;

                            // Add property value to name, if exists
                            if (GeneralHelper.IsNotNull(prop))
                            {
                                if (component.Name == "ResourceInstance")
                                {
                                    value = prop;
                                }
                                else
                                {
                                    value = prop.GetType().GetProperty("ShortName").GetValue(prop, null).ToLower();
                                }

                                // Check if the delimeter is already ignored
                                if (!ignoredelimeter)
                                {
                                    // Check if delimeter is an invalid character
                                    if (!String.IsNullOrEmpty(resourceType.InvalidCharacters))
                                    {
                                        if (!resourceType.InvalidCharacters.Contains(request.ResourceDelimiter.Delimiter))
                                        {
                                            if (!String.IsNullOrEmpty(name))
                                            {
                                                name += request.ResourceDelimiter.Delimiter;
                                            }
                                        }
                                        else
                                        {
                                            // Add message about delimeter not applied
                                            sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed.");
                                            sbMessage.Append(Environment.NewLine);
                                            ignoredelimeter = true;
                                        }
                                    }
                                    else
                                    {
                                        // Deliemeter is valid so add it
                                        if (!String.IsNullOrEmpty(name))
                                        {
                                            name += request.ResourceDelimiter.Delimiter;
                                        }
                                    }
                                }

                                name += value;

                                // Add property to aray for indivudal component validation
                                if (component.Name == "ResourceType")
                                {
                                    lstComponents.Add(new string[] { component.Name, prop.Resource + " (" + value + ")" });
                                }
                                else
                                {
                                    if (component.Name == "ResourceInstance")
                                    {
                                        lstComponents.Add(new string[] { component.Name, prop });
                                    }
                                    else
                                    {
                                        lstComponents.Add(new string[] { component.Name, prop.Name + " (" + value + ")" });
                                    }
                                }
                            }
                            else
                            {
                                // Check if the prop is optional
                                if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                // Check if the required component were supplied
                if (!valid)
                {
                    response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    response.Message = "You must supply the required components.";
                    return response;
                }

                // Check the Resource Instance value to ensure it's only nmumeric
                if (GeneralHelper.IsNotNull(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")))
                {
                    if (GeneralHelper.IsNotNull(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                    {
                        if (!ValidationHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                        {
                            sbMessage.Append("Resource Instance must be a numeric value.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Validate the generated name for the resource type
                // CALL VALIDATION FUNCTION
                ValidateNameRequest validateNameRequest = new ValidateNameRequest()
                {
                    ResourceType = resourceType.ShortName,
                    Name = name
                };
                serviceResponse = await ResourceTypeService.ValidateResourceTypeName(validateNameRequest);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        ValidateNameResponse validateNameResponse = (ValidateNameResponse)serviceResponse.ResponseObject!;
                        valid = validateNameResponse.Valid;
                        if (!String.IsNullOrEmpty(validateNameResponse.Name))
                        {
                            name = validateNameResponse.Name;
                        }
                        if (!String.IsNullOrEmpty(validateNameResponse.Message))
                        {
                            sbMessage.Append(validateNameResponse.Message);
                        }
                    }
                }

                if (valid)
                {
                    GeneratedName generatedName = new()
                    {
                        CreatedOn = DateTime.Now,
                        ResourceName = name.ToLower(),
                        Components = lstComponents
                    };
                    await GeneratedNamesService.PostItem(generatedName);
                    response.Success = true;
                    response.ResourceName = name.ToLower();
                    response.Message = sbMessage.ToString();
                    return response;
                }
                else
                {
                    response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    response.Message = sbMessage.ToString();
                    return response;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                response.Message = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// This function is used to generate a name by providing each componetn and the short name value. The function will validate the values to ensure they match the current configuration. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ResourceNameResponse - Response of name generation</returns>
        public static async Task<ResourceNameResponse> RequestName(ResourceNameRequest request)
        {
            ResourceNameResponse resourceNameResponse = new()
            {
                Success = false
            };

            try
            {
                bool valid = true;
                bool ignoredelimeter = false;
                List<string[]> lstComponents = new();
                ServiceResponse serviceResponse = new();
                ResourceDelimiter resourceDelimiter = new();
                ResourceType resourceType = new();
                string name = "";
                StringBuilder sbMessage = new();

                // Get the current delimiter
                serviceResponse = await ResourceDelimiterService.GetCurrentItem();
                if (serviceResponse.Success)
                {
                    resourceDelimiter = (ResourceDelimiter)serviceResponse.ResponseObject!;
                }
                else
                {
                    valid = false;
                    resourceNameResponse.Message = "Delimiter value could not be set.";
                    resourceNameResponse.Success = false;
                    return resourceNameResponse;
                }

                // Get the specified resource type
                var resourceTypes = await ConfigurationHelper.GetList<ResourceType>();
                if (GeneralHelper.IsNotNull(resourceTypes))
                {
                    var resourceTypesByShortName = resourceTypes.FindAll(x => x.ShortName == request.ResourceType);
                    if (!GeneralHelper.IsNotNull(resourceTypesByShortName))
                    {
                        valid = false;
                        resourceNameResponse.Message = "ResourceType value is invalid.";
                        resourceNameResponse.Success = false;
                        return resourceNameResponse;
                    }
                    else
                    {
                        if (resourceTypesByShortName.Count == 0)
                        {
                            valid = false;
                            resourceNameResponse.Message = "ResourceType value is invalid.";
                            resourceNameResponse.Success = false;
                            return resourceNameResponse;
                        }
                        // Check if there are duplicates
                        if (resourceTypesByShortName.Count > 1)
                        {
                            // Check that the request includes a resource name
                            if (request.ResourceId != 0)
                            {
                                // Check if the resource value is valid
                                resourceType = resourceTypesByShortName.Find(x => x.Id == request.ResourceId)!;
                                if (!GeneralHelper.IsNotNull(resourceType))
                                {
                                    valid = false;
                                    resourceNameResponse.Message = "Resource Id value is invalid.";
                                    resourceNameResponse.Success = false;
                                    return resourceNameResponse;
                                }
                            }
                            else
                            {
                                valid = false;
                                resourceNameResponse.Message = "Your configuration contains multiple resource types for the provided short name. You must supply the Resource Id value for the resource type in your request.(Example: resourceId: 14)";
                                resourceNameResponse.Success = false;
                                return resourceNameResponse;
                            }
                        }
                        else
                        {
                            // Set the resource type ot the first value
                            resourceType = resourceTypesByShortName[0];
                        }
                    }

                    // Check static value
                    if (!String.IsNullOrEmpty(resourceType.StaticValues))
                    {
                        // Return the static value and message and stop generation.
                        resourceNameResponse.ResourceName = resourceType.StaticValues;
                        resourceNameResponse.Message = "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                        resourceNameResponse.Success = true;
                        return resourceNameResponse;
                    }

                    // Make sure the passed custom component names are normalized
                    if (GeneralHelper.IsNotNull(request.CustomComponents))
                    {
                        Dictionary<string, string> newComponents = new();
                        foreach (var cc in request.CustomComponents)
                        {
                            string value = cc.Value;
                            newComponents.Add(GeneralHelper.NormalizeName(cc.Key, true), value);
                        }
                        request.CustomComponents = newComponents;
                    }

                    // Get the current components
                    serviceResponse = await ResourceComponentService.GetItems(false);
                    if (serviceResponse.Success)
                    {
                        if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                        {
                            var currentResourceComponents = serviceResponse.ResponseObject;
                            if (GeneralHelper.IsNotNull(currentResourceComponents))
                            {
                                // Loop through each component
                                foreach (var component in currentResourceComponents!)
                                {
                                    string normalizedcomponentname = GeneralHelper.NormalizeName(component.Name, true);
                                    if (!component.IsCustom)
                                    {
                                        // Check if the component is excluded for the Resource Type
                                        if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                        {
                                            // Attempt to retrieve value from JSON body
                                            var value = GeneralHelper.GetPropertyValue(request, component.Name);

                                            // Add property value to name, if exists
                                            if (GeneralHelper.IsNotNull(value))
                                            {
                                                if (!String.IsNullOrEmpty(value))
                                                {
                                                    // Validate that the value is a valid option for the component
                                                    switch (component.Name.ToLower())
                                                    {
                                                        case "resourcetype":
                                                            var types = await ConfigurationHelper.GetList<ResourceType>();
                                                            if (GeneralHelper.IsNotNull(types))
                                                            {
                                                                var type = types.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(type))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceType value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourceenvironment":
                                                            var environments = await ConfigurationHelper.GetList<ResourceEnvironment>();
                                                            if (GeneralHelper.IsNotNull(environments))
                                                            {
                                                                var environment = environments.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(environment))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceEnvironment value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourcelocation":
                                                            var locations = await ConfigurationHelper.GetList<ResourceLocation>();
                                                            if (GeneralHelper.IsNotNull(locations))
                                                            {
                                                                var location = locations.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(location))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceLocation value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourceorg":
                                                            var orgs = await ConfigurationHelper.GetList<ResourceOrg>();
                                                            if (GeneralHelper.IsNotNull(orgs))
                                                            {
                                                                var org = orgs.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(org))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("Resource Type value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourceprojappsvc":
                                                            var projappsvcs = await ConfigurationHelper.GetList<ResourceProjAppSvc>();
                                                            if (GeneralHelper.IsNotNull(projappsvcs))
                                                            {
                                                                var projappsvc = projappsvcs.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(projappsvc))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceProjAppSvc value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourceunitdept":
                                                            var unitdepts = await ConfigurationHelper.GetList<ResourceUnitDept>();
                                                            if (GeneralHelper.IsNotNull(unitdepts))
                                                            {
                                                                var unitdept = unitdepts.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(unitdept))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceUnitDept value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                        case "resourcefunction":
                                                            var functions = await ConfigurationHelper.GetList<ResourceFunction>();
                                                            if (GeneralHelper.IsNotNull(functions))
                                                            {
                                                                var function = functions.Find(x => x.ShortName == value);
                                                                if (!GeneralHelper.IsNotNull(function))
                                                                {
                                                                    valid = false;
                                                                    sbMessage.Append("ResourceFunction value is invalid. ");
                                                                }
                                                            }
                                                            break;
                                                    }
                                                    //var items = await ConfigurationHelper.GetList<ResourceComponent>();

                                                    // Check if the delimeter is already ignored
                                                    if ((!ignoredelimeter) && (!String.IsNullOrEmpty(resourceDelimiter.Delimiter)))
                                                    {
                                                        // Check if delimeter is an invalid character
                                                        if (!String.IsNullOrEmpty(resourceType.InvalidCharacters))
                                                        {
                                                            if (!resourceType.InvalidCharacters.Contains(resourceDelimiter.Delimiter))
                                                            {
                                                                if (name != "")
                                                                {
                                                                    name += resourceDelimiter.Delimiter;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Add message about delimeter not applied
                                                                sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed. ");
                                                                ignoredelimeter = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Deliemeter is valid so add it
                                                            if (!String.IsNullOrEmpty(name))
                                                            {
                                                                name += resourceDelimiter.Delimiter;
                                                            }
                                                        }
                                                    }

                                                    name += value;

                                                    // Add property to array for individual component validation
                                                    if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                                    {
                                                        lstComponents.Add(new string[] { component.Name, value });
                                                    }
                                                }
                                                else
                                                {
                                                    // Check if the prop is optional
                                                    if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                    {
                                                        valid = false;
                                                        sbMessage.Append(component.Name + " value was not provided. ");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Check if the prop is optional
                                                if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                {
                                                    valid = false;
                                                    sbMessage.Append(component.Name + " value was not provided. ");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!component.IsFreeText)
                                        {
                                            // Get the custom components data
                                            serviceResponse = await CustomComponentService.GetItems();
                                            if (serviceResponse.Success)
                                            {
                                                if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                                                {
                                                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                                                    {
                                                        var customcomponents = (List<CustomComponent>)serviceResponse.ResponseObject!;
                                                        if (GeneralHelper.IsNotNull(customcomponents))
                                                        {
                                                            // Make sure the custom component has values
                                                            if (customcomponents.Where(x => x.ParentComponent == normalizedcomponentname).Any())
                                                            {
                                                                // Make sure the CustomComponents property was provided
                                                                if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                                                {
                                                                    // Add property value to name, if exists
                                                                    if (GeneralHelper.IsNotNull(request.CustomComponents))
                                                                    {
                                                                        // Check if the custom compoment value was provided in the request
                                                                        if (request.CustomComponents.ContainsKey(normalizedcomponentname))
                                                                        {
                                                                            // Get the value from the provided custom components
                                                                            var componentvalue = request.CustomComponents[normalizedcomponentname];
                                                                            if (!GeneralHelper.IsNotNull(componentvalue))
                                                                            {
                                                                                // Check if the prop is optional
                                                                                if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                                                {
                                                                                    valid = false;
                                                                                    sbMessage.Append(component.Name + " value was not provided. ");
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                // Check to make sure it is a valid custom component
                                                                                var customComponents = await ConfigurationHelper.GetList<CustomComponent>();
                                                                                if (GeneralHelper.IsNotNull(customComponents))
                                                                                {
                                                                                    var validcustomComponent = customComponents.Find(x => x.ParentComponent == normalizedcomponentname && x.ShortName == componentvalue);
                                                                                    if (!GeneralHelper.IsNotNull(validcustomComponent))
                                                                                    {
                                                                                        valid = false;
                                                                                        sbMessage.Append(component.Name + " value is not a valid custom component short name. ");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (!String.IsNullOrEmpty(name))
                                                                                        {
                                                                                            name += resourceDelimiter.Delimiter;
                                                                                        }

                                                                                        name += componentvalue;

                                                                                        // Add property to array for individual component validation
                                                                                        lstComponents.Add(new string[] { component.Name, componentvalue });
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // Check if the prop is optional
                                                                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                                            {
                                                                                valid = false;
                                                                                sbMessage.Append(component.Name + " value was not provided. ");
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Check if the prop is optional
                                                                        if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                                        {
                                                                            valid = false;
                                                                            sbMessage.Append(component.Name + " value was not provided. ");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Make sure the CustomComponents property was provided
                                            if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                            {
                                                // Add property value to name, if exists
                                                if (GeneralHelper.IsNotNull(request.CustomComponents))
                                                {
                                                    // Check if the custom compoment value was provided in the request
                                                    if (request.CustomComponents.ContainsKey(normalizedcomponentname))
                                                    {
                                                        // Get the value from the provided custom components
                                                        var componentvalue = request.CustomComponents[normalizedcomponentname];
                                                        if (!GeneralHelper.IsNotNull(componentvalue))
                                                        {
                                                            // Check if the prop is optional
                                                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                            {
                                                                valid = false;
                                                                sbMessage.Append(component.Name + " value was not provided. ");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!String.IsNullOrEmpty(name))
                                                            {
                                                                name += resourceDelimiter.Delimiter;
                                                            }

                                                            name += componentvalue;

                                                            // Add property to array for individual component validation
                                                            lstComponents.Add(new string[] { component.Name, componentvalue });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Check if the prop is optional
                                                        if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                        {
                                                            valid = false;
                                                            sbMessage.Append(component.Name + " value was not provided. ");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // Check if the prop is optional
                                                    if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                    {
                                                        valid = false;
                                                        sbMessage.Append(component.Name + " value was not provided. ");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    valid = false;
                    sbMessage.Append("There was a problem generating the name.");
                }

                // Check if the required component were supplied
                if (!valid)
                {
                    resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    resourceNameResponse.Message = "You must supply the required components. " + sbMessage.ToString();
                    return resourceNameResponse;
                }

                // Check the Resource Instance value to ensure it's only nmumeric
                if (GeneralHelper.IsNotNull(lstComponents))
                {
                    if (GeneralHelper.IsNotNull(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")))
                    {
                        if (GeneralHelper.IsNotNull(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                        {
                            if (!ValidationHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")![1]))
                            {
                                sbMessage.Append("Resource Instance must be a numeric value.");
                                sbMessage.Append(Environment.NewLine);
                                valid = false;
                            }
                        }
                    }
                }

                // Validate the generated name for the resource type
                ValidateNameRequest validateNameRequest = new ValidateNameRequest()
                {
                    ResourceType = resourceType.ShortName,
                    Name = name
                };
                serviceResponse = await ResourceTypeService.ValidateResourceTypeName(validateNameRequest);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        ValidateNameResponse validateNameResponse = (ValidateNameResponse)serviceResponse.ResponseObject!;
                        valid = validateNameResponse.Valid;
                        if (!String.IsNullOrEmpty(validateNameResponse.Name))
                        {
                            name = validateNameResponse.Name;
                        }
                        if (!String.IsNullOrEmpty(validateNameResponse.Message))
                        {
                            sbMessage.Append(validateNameResponse.Message);
                        }
                    }
                }

                if (valid)
                {
                    bool nameallowed = true;
                    // Check if duplicate names are allowed
                    if (!ConfigurationHelper.VerifyDuplicateNamesAllowed())
                    {
                        // Check if the name already exists
                        serviceResponse = await GeneratedNamesService.GetItems();
                        if (serviceResponse.Success)
                        {
                            if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                            {
                                var names = (List<GeneratedName>)serviceResponse.ResponseObject!;
                                if (GeneralHelper.IsNotNull(names))
                                {
                                    if (names.Where(x => x.ResourceName == name).Any())
                                    {
                                        nameallowed = false;
                                    }
                                }
                            }
                        }
                    }
                    if (nameallowed)
                    {
                        GeneratedName generatedName = new()
                        {
                            CreatedOn = DateTime.Now,
                            ResourceName = name.ToLower(),
                            Components = lstComponents,
                            ResourceTypeName = resourceType.Resource,
                            User = request.CreatedBy
                        };

                        // Check if the property should be appended to name
                        if(!String.IsNullOrEmpty(resourceType.Property))
                        {
                            generatedName.ResourceTypeName += " - " + resourceType.Property;
                        }

                        ServiceResponse responseGenerateName = await GeneratedNamesService.PostItem(generatedName);
                        if (responseGenerateName.Success)
                        {
                            resourceNameResponse.Success = true;
                            resourceNameResponse.ResourceName = name.ToLower();
                            resourceNameResponse.Message = sbMessage.ToString();
                            resourceNameResponse.ResourceNameDetails = generatedName;

                            // Check if the GenerationWebhook is configured
                            String webhook = ConfigurationHelper.GetAppSetting("GenerationWebhook", true);
                            if (!String.IsNullOrEmpty(webhook))
                            {
                                // Asynchronously post to the webhook
                                await ConfigurationHelper.PostToGenerationWebhook(webhook, generatedName);
                            }
                        }
                        else
                        {
                            resourceNameResponse.Success = false;
                            resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                            resourceNameResponse.Message = "There was an error generating the name. Please try again.";
                        }
                    }
                    else
                    {
                        resourceNameResponse.Success = false;
                        resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                        resourceNameResponse.Message = "The name (" + name + ") you are trying to generate already exists. Please select different component options and try again.";
                    }
                    return resourceNameResponse;
                }
                else
                {
                    resourceNameResponse.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    resourceNameResponse.Message = sbMessage.ToString();
                    return resourceNameResponse;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                resourceNameResponse.Message = ex.Message;
                return resourceNameResponse;
            }
        }
    }
}

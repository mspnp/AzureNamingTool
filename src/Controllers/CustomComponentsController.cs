#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;
using Microsoft.Extensions.Options;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing custom components.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class CustomComponentsController : ControllerBase
    {
        private readonly ICustomComponentService _customComponentService;
        private readonly IResourceComponentService _resourceComponentService;
        private readonly IAdminLogService _adminLogService;

        public CustomComponentsController(
            ICustomComponentService customComponentService,
            IResourceComponentService resourceComponentService,
            IAdminLogService adminLogService)
        {
            _customComponentService = customComponentService;
            _resourceComponentService = resourceComponentService;
            _adminLogService = adminLogService;
        }

        /// <summary>
        /// Response for controller functions
        /// </summary>
        ServiceResponse serviceResponse = new();

        // GET: api/<CustomComponentsController>
        /// <summary>
        /// This function will return the custom components data. 
        /// </summary>
        /// <returns>json - Current custom components data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await _customComponentService.GetItemsAsync();
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // GET api/<CustomComponentsController>/sample
        /// <summary>
        /// This function will return the custom components data for the specifc parent component id.
        /// </summary>
        /// <param name="parentcomponentid">int - Parent Component Id</param>
        /// <returns>json - Current custom components data</returns>
        [Route("[action]/{parentcomponentid}")]
        [HttpGet]
        public async Task<IActionResult> GetByParentComponentId(int parentcomponentid)
        {
            try
            {
                // Get list of items
                serviceResponse = await _customComponentService.GetItemsByParentComponentIdAsync(parentcomponentid);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // GET api/<CustomComponentsController>/sample
        /// <summary>
        /// This function will return the custom components data for the specifc parent component type (name).
        /// </summary>
        /// <param name="parenttype">string - Parent Component Type Name</param>
        /// <returns>json - Current custom components data</returns>
        [Route("[action]/{parenttype}")]
        [HttpGet]
        public async Task<IActionResult> GetByParentType(string parenttype)
        {
            try
            {
                // Get list of items
                serviceResponse = await _customComponentService.GetItemsByParentTypeAsync(GeneralHelper.NormalizeName(parenttype, true));
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // GET api/<CustomComponentsController>/5
        /// <summary>
        /// This function will return the specifed custom component data.
        /// </summary>
        /// <param name="id">int - Custom Component id</param>
        /// <returns>json - Custom component data</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _customComponentService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will create/update the specified custom component data.
        /// </summary>
        /// <param name="item">CustomComponent (json) - Custom component data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomComponent item)
        {
            try
            {
                serviceResponse = await _customComponentService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Custom Component (" + item.Name + ") updated." });
                    CacheHelper.InvalidateCacheObject("CustomComponent");
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will update all custom components data.
        /// </summary>
        /// <param name="items">List-CustomComponent (json) - All custom components data. (Legacy functionality).</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<CustomComponent> items)
        {
            try
            {
                serviceResponse = await _customComponentService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Custom Components updated." });
                    CacheHelper.InvalidateCacheObject("CustomComponent");
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will update all custom components data.
        /// </summary>
        /// <param name="config">CustomComponmentConfig (json) - Full custom components data with parent component data.</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfigWithParentData([FromBody] CustomComponmentConfig config)
        {
            try
            {
                List<ResourceComponent> currentresourcecomponents = [];
                List<CustomComponent> newcustomcomponents = [];
                // Get the current resource components
                serviceResponse = await _resourceComponentService.GetItemsAsync(true);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        currentresourcecomponents = serviceResponse.ResponseObject!;

                        // Loop through the posted components
                        if (GeneralHelper.IsNotNull(config.ParentComponents))
                        {
                            foreach (ResourceComponent thisparentcomponent in config.ParentComponents)
                            {
                                // Check if the posted component exists in the current components
                                if (!currentresourcecomponents.Exists(x => x.Name == thisparentcomponent.Name))
                                {
                                    // Add the custom component
                                    ResourceComponent newcustomcomponent = new()
                                    {
                                        Name = thisparentcomponent.Name,
                                        DisplayName = thisparentcomponent.Name,
                                        IsCustom = true
                                    };
                                    serviceResponse = await _resourceComponentService.PostItemAsync(newcustomcomponent);

                                    if (serviceResponse.Success)
                                    {
                                        // Add the new custom component to the list
                                        currentresourcecomponents.Add(newcustomcomponent);
                                    }
                                    else
                                    {
                                        return BadRequest(serviceResponse.ResponseObject);
                                    }
                                }
                            }
                        }
                    }

                    if (GeneralHelper.IsNotNull(config.CustomComponents))
                    {
                        if (config.CustomComponents.Count > 0)
                        {
                            // Loop through custom components to make sure the parent exists
                            foreach (CustomComponent thiscustomcomponent in config.CustomComponents)
                            {
                                if (currentresourcecomponents.Where(x => GeneralHelper.NormalizeName(x.Name, true) == thiscustomcomponent.ParentComponent).Any())
                                {
                                    newcustomcomponents.Add(thiscustomcomponent);
                                }
                            }

                            // Update the custom component options
                            serviceResponse = await _customComponentService.PostConfigAsync(newcustomcomponents);
                            if (!serviceResponse.Success)
                            {
                                return BadRequest(serviceResponse.ResponseObject);
                            }
                        }
                    }
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Custom Components updated." });
                    CacheHelper.InvalidateCacheObject("CustomComponent");
                    return Ok("Custom Component configuration updated!");
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // DELETE api/<CustomComponentsController>/5
        /// <summary>
        /// This function will delete the specifed custom component data.
        /// </summary>
        /// <param name="id">int - Custom component id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _customComponentService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    CustomComponent item = (CustomComponent)serviceResponse.ResponseObject!;
                    serviceResponse = await _customComponentService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Custom Component (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("CustomComponent");
                        return Ok("Custom Component (" + item.Name + ") deleted.");
                    }
                    else
                    {
                        return BadRequest(serviceResponse.ResponseObject);
                    }
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }
        // DELETE api/<CustomComponentsController>/5
        /// <summary>
        /// This function will delete the custom component data for the specifed parent component id.
        /// </summary>
        /// <param name="parentcomponentid">int - Parent component id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("[action]/{parentcomponentid}")]
        public async Task<IActionResult> DeleteByParentComponentId(int parentcomponentid)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceComponentService.GetItemAsync(parentcomponentid);
                if (serviceResponse.Success)
                {
                    var component = (ResourceComponent)serviceResponse.ResponseObject!;
                    serviceResponse = await _customComponentService.DeleteByParentComponentIdAsync(parentcomponentid);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Custom Component data for component (" + component.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("CustomComponent");
                        return Ok("Custom Component data for component (" + component.Name + ") deleted.");
                    }
                    else
                    {
                        return BadRequest(serviceResponse.ResponseObject);
                    }
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }
    }
}
#pragma warning restore CS1591
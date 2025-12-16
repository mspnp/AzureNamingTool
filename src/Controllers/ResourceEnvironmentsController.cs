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

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource environments.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceEnvironmentsController : ControllerBase
    {
        private readonly IResourceEnvironmentService _resourceEnvironmentService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();

        public ResourceEnvironmentsController(
            IResourceEnvironmentService resourceEnvironmentService,
            IAdminLogService adminLogService)
        {
            _resourceEnvironmentService = resourceEnvironmentService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceEnvironmentsController>
        /// <summary>
        /// This function will return the environments data. 
        /// </summary>
        /// <returns>json - Current environments data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                serviceResponse = await _resourceEnvironmentService.GetItemsAsync();
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

        // GET api/<ResourceEnvironmentsController>/5
        /// <summary>
        /// This function will return the specifed environment data.
        /// </summary>
        /// <param name="id">int - Environment id</param>
        /// <returns>json - Environment data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                serviceResponse = await _resourceEnvironmentService.GetItemAsync(id);
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

        // POST api/<ResourceEnvironmentsController>
        /// <summary>
        /// This function will create/update the specified environment data.
        /// </summary>
        /// <param name="item">ResourceEnvironment (json) - Environment data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceEnvironment item)
        {
            try
            {
                serviceResponse = await _resourceEnvironmentService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Environment (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceEnvironment");
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

        // POST api/<ResourceEnvironmentsController>
        /// <summary>
        /// This function will update all environments data.
        /// </summary>
        /// <param name="items">List - ResourceEnvironment (json) - All environments data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceEnvironment> items)
        {
            try
            {
                serviceResponse = await _resourceEnvironmentService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Environments added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceEnvironment");
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

        // DELETE api/<ResourceEnvironmentsController>/5
        /// <summary>
        /// This function will delete the specifed environment data.
        /// </summary>
        /// <param name="id">int - Environment id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceEnvironmentService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    ResourceEnvironment item = (ResourceEnvironment)serviceResponse.ResponseObject!;
                    serviceResponse = await _resourceEnvironmentService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Environment (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("ResourceEnvironment");
                        return Ok("Resource Environment (" + item.Name + ") deleted.");
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
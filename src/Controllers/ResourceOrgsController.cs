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
    /// Controller for managing Resource Orgs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceOrgsController : ControllerBase
    {
        private readonly IResourceOrgService _resourceorgService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();
        public ResourceOrgsController(
            IResourceOrgService resourceorgService,
            IAdminLogService adminLogService)
        {
            _resourceorgService = resourceorgService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceOrgsController>
        /// <summary>
        /// This function will return the orgs data. 
        /// </summary>
        /// <returns>json - Current orgs data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceorgService.GetItemsAsync();
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

        // GET api/<ResourceOrgsController>/5
        /// <summary>
        /// This function will return the specifed org data.
        /// </summary>
        /// <param name="id">int - Org id</param>
        /// <returns>json - Org data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceorgService.GetItemAsync(id);
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

        // POST api/<ResourceOrgsController>
        /// <summary>
        /// This function will create/update the specified org data.
        /// </summary>
        /// <param name="item">ResourceOrg (json) - Org data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceOrg item)
        {
            try
            {
                serviceResponse = await _resourceorgService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Org (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceOrg");
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

        // POST api/<ResourceOrgsController>
        /// <summary>
        /// This function will update all orgs data.
        /// </summary>
        /// <param name="items">List - ResourceOrg (json) - All orgs data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceOrg> items)
        {
            try
            {
                serviceResponse = await _resourceorgService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Orgs added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceOrg");
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

        // DELETE api/<ResourceOrgsController>/5
        /// <summary>
        /// This function will delete the specifed org data.
        /// </summary>
        /// <param name="id">int - Org id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceorgService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    ResourceOrg item = (ResourceOrg)serviceResponse.ResponseObject!;
                    serviceResponse = await _resourceorgService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Org (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("ResourceOrg");
                        return Ok("Resource Org (" + item.Name + ") deleted.");
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
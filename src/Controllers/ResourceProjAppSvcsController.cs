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
    /// Controller for managing resource projects, apps, and services.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceProjAppSvcsController : ControllerBase
    {
        private readonly IResourceProjAppSvcService _resourceprojappsvcService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();
        public ResourceProjAppSvcsController(
            IResourceProjAppSvcService resourceprojappsvcService,
            IAdminLogService adminLogService)
        {
            _resourceprojappsvcService = resourceprojappsvcService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceProjAppSvcsController>
        /// <summary>
        /// This function will return the projects/apps/services data. 
        /// </summary>
        /// <returns>json - Current projects/apps/services data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                serviceResponse = await _resourceprojappsvcService.GetItemsAsync();
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

        // GET api/<ResourceProjAppSvcsController>/5
        /// <summary>
        /// This function will return the specified project/app/service data.
        /// </summary>
        /// <param name="id">int - Project/App/Service id</param>
        /// <returns>json - Project/App/Service data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceprojappsvcService.GetItemAsync(id);
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

        // POST api/<ResourceProjAppSvcsController>
        /// <summary>
        /// This function will create/update the specified project/app/service data.
        /// </summary>
        /// <param name="item">ResourceProjAppSvc (json) - Project/App/Service data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceProjAppSvc item)
        {
            try
            {
                serviceResponse = await _resourceprojappsvcService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Project/App/Service (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
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

        // POST api/<ResourceProjAppSvcsController>
        /// <summary>
        /// This function will update all projects/apps/services data.
        /// </summary>
        /// <param name="items">List - ResourceProjAppSvc (json) - All projects/apps/services data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceProjAppSvc> items)
        {
            try
            {
                serviceResponse = await _resourceprojappsvcService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Projects/Apps/Services added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
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

        // DELETE api/<ResourceProjAppSvcsController>/5
        /// <summary>
        /// This function will delete the specified project/app/service data.
        /// </summary>
        /// <param name="id">int - Project/App/Service id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceprojappsvcService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    ResourceProjAppSvc item = (ResourceProjAppSvc)serviceResponse.ResponseObject!;
                    serviceResponse = await _resourceprojappsvcService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Project/App/Service (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
                        return Ok("Resource Project/App/Service (" + item.Name + ") deleted.");
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
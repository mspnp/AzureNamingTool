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
    /// Controller for managing resource functions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceFunctionsController : ControllerBase
    {
        private readonly IResourceFunctionService _resourceFunctionService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();

        public ResourceFunctionsController(
            IResourceFunctionService resourceFunctionService,
            IAdminLogService adminLogService)
        {
            _resourceFunctionService = resourceFunctionService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceFunctionsController>
        /// <summary>
        /// This function will return the functions data. 
        /// </summary>
        /// <returns>json - Current functions data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceFunctionService.GetItemsAsync();
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

        // GET api/<ResourceFunctionsController>/5
        /// <summary>
        /// This function will return the specifed function data.
        /// </summary>
        /// <param name="id">int - Function id</param>
        /// <returns>json - Function data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                serviceResponse = await _resourceFunctionService.GetItemAsync(id);
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

        // POST api/<ResourceFunctionsController>
        /// <summary>
        /// This function will create/update the specified function data.
        /// </summary>
        /// <param name="item">ResourceFunction (json) - Function data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceFunction item)
        {
            try
            {
                serviceResponse = await _resourceFunctionService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Function (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceFunction");
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

        // POST api/<ResourceFunctionsController>
        /// <summary>
        /// This function will update all functions data.
        /// </summary>
        /// <param name="items">List - ResourceFunction (json) - All functions data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceFunction> items)
        {
            try
            {
                serviceResponse = await _resourceFunctionService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Functions added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceFunction");
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

        // DELETE api/<ResourceFunctionsController>/5
        /// <summary>
        /// This function will delete the specifed function data.
        /// </summary>
        /// <param name="id">int - Function id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceFunctionService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    ResourceFunction item = (ResourceFunction)serviceResponse.ResponseObject!;
                    serviceResponse = await _resourceFunctionService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Function (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("ResourceFunction");
                        return Ok("Resource Function (" + item.Name + ") deleted.");
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
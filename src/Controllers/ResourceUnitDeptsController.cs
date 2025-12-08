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
    /// Controller for managing resource units/departments.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceUnitDeptsController : ControllerBase
    {
        private readonly IResourceUnitDeptService _resourceunitdeptService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();
        public ResourceUnitDeptsController(
            IResourceUnitDeptService resourceunitdeptService,
            IAdminLogService adminLogService)
        {
            _resourceunitdeptService = resourceunitdeptService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceUnitDeptsController>
        /// <summary>
        /// This function will return the units/depts data. 
        /// </summary>
        /// <returns>json - Current units/depts data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceunitdeptService.GetItemsAsync();
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

        // GET api/<ResourceUnitDeptsController>/5
        /// <summary>
        /// This function will return the specifed unit/dept data.
        /// </summary>
        /// <param name="id">int - Unit/Dept id</param>
        /// <returns>json - Unit/Dept data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceunitdeptService.GetItemAsync(id);
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

        // POST api/<ResourceUnitDeptsController>
        /// <summary>
        /// This function will create/update the specified unit/dept data.
        /// </summary>
        /// <param name="item">ResourceUnitDept (json) - Unit/Dept data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceUnitDept item)
        {
            try
            {
                serviceResponse = await _resourceunitdeptService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Unit/Department (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceUnitDept");
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

        // POST api/<ResourceUnitDeptsController>
        /// <summary>
        /// This function will update all units/depts data.
        /// </summary>
        /// <param name="items">List - ResourceUnitDept (json) - All units/depts data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceUnitDept> items)
        {
            try
            {
                serviceResponse = await _resourceunitdeptService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Units/Departments added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceUnitDept");
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

        // DELETE api/<ResourceUnitDeptsController>/5
        /// <summary>
        /// This function will delete the specifed unit/dept data.
        /// </summary>
        /// <param name="id">int - Unit/Dept id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the item details
                serviceResponse = await _resourceunitdeptService.GetItemAsync(id);
                if (serviceResponse.Success)
                {
                    ResourceUnitDept item = (ResourceUnitDept)serviceResponse.ResponseObject!;
                    serviceResponse = await _resourceunitdeptService.DeleteItemAsync(id);
                    if (serviceResponse.Success)
                    {
                        await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Unit/Department (" + item.Name + ") deleted." });
                        CacheHelper.InvalidateCacheObject("ResourceUnitDept");
                        return Ok("Resource Unit/Department (" + item.Name + ") deleted.");
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
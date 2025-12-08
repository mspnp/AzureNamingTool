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
    /// Controller for managing resource locations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceLocationsController : ControllerBase
    {
        private readonly IResourceLocationService _resourceLocationService;
        private readonly IAdminLogService _adminLogService;
        private ServiceResponse serviceResponse = new();

        public ResourceLocationsController(
            IResourceLocationService resourceLocationService,
            IAdminLogService adminLogService)
        {
            _resourceLocationService = resourceLocationService;
            _adminLogService = adminLogService;
        }

        // GET: api/<ResourceLocationsController>
        /// <summary>
        /// This function will return the locations data. 
        /// </summary>
        /// <param name="admin">bool - Indicates if the user is an admin</param>
        /// <returns>json - Current locations data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                serviceResponse = await _resourceLocationService.GetItemsAsync(admin);
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

        // GET api/<ResourceLocationsController>/5
        /// <summary>
        /// This function will return the specified location data.
        /// </summary>
        /// <param name="id">int - Location id</param>
        /// <returns>json - Location data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                serviceResponse = await _resourceLocationService.GetItemAsync(id);
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

        // POST api/<ResourceLocationsController>
        /// <summary>
        /// This function will update all locations data.
        /// </summary>
        /// <param name="items">List - ResourceLocation (json) - All locations data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceLocation> items)
        {
            try
            {
                serviceResponse = await _resourceLocationService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Locations added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceLocation");
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
    }
}

#pragma warning restore CS1591
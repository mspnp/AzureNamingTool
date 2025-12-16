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
using System.Web;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource types.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceTypesController : ControllerBase
    {
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;

        public ResourceTypesController(
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService)
        {
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
        }

        private ServiceResponse serviceResponse = new();

        // GET: api/<ResourceTypesController>
        /// <summary>
        /// This function will return the resource types data. 
        /// </summary>
        /// <param name="admin">bool - Indicates if the user is an admin (optional)</param>
        /// <returns>json - Current resource types data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Models.ResourceType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceTypeService.GetItemsAsync(admin);
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

        // GET api/<ResourceTypesController>/5
        /// <summary>
        /// This function will return the specified resource type data.
        /// </summary>
        /// <param name="id">int - Resource Type id</param>
        /// <returns>json - Resource Type data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Models.ResourceType), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceTypeService.GetItemAsync(id);
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

        // POST api/<ResourceTypesController>
        /// <summary>
        /// This function will update all resource types data.
        /// </summary>
        /// <param name="items">List - ResourceType (json) - All resource types data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceType> items)
        {
            try
            {
                serviceResponse = await _resourceTypeService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Types updated." });
                    CacheHelper.InvalidateCacheObject("ResourceType");
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

        // POST api/<ResourceTypesController>
        /// <summary>
        /// This function will update all resource types for the specified component.
        /// </summary>
        /// <param name="operation">string - Operation type (optional-add, optional-remove, exlcude-add, exclude-remove)</param>
        /// <param name="componentid">int - Component ID</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateTypeComponents(string operation, int componentid)
        {
            try
            {
                serviceResponse = await _resourceTypeService.UpdateTypeComponentsAsync(operation, componentid);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Types updated." });
                    CacheHelper.InvalidateCacheObject("ResourceType");
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
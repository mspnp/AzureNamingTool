#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using PSC.Blazor.Components.MarkdownEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource components.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceComponentsController : ControllerBase
    {
        private readonly IResourceComponentService _resourceComponentService;
        private readonly IAdminLogService _adminLogService;
        /// <summary>
        /// Response for controller functions
        /// </summary>
        ServiceResponse serviceResponse = new();

        public ResourceComponentsController(
            IResourceComponentService resourceComponentService,
            IAdminLogService adminLogService)
        {
            _resourceComponentService = resourceComponentService;
            _adminLogService = adminLogService;
        }

        // GET: api/<resourcecomponentsController>
        /// <summary>
        /// This function will return the components data.
        /// </summary>
        /// <param name="admin">bool - All/Only-enabled components flag</param>
        /// <returns>json - Current components data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                serviceResponse = await _resourceComponentService.GetItemsAsync(admin);
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

        // GET api/<resourcecomponentsController>/5
        /// <summary>
        /// This function will return the specified resource component data.
        /// </summary>
        /// <param name="id">int - Resource Component id</param>
        /// <returns>json - Resource component data</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceComponentService.GetItemAsync(id);
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

        // POST api/<ResourceComponentsController>
        /// <summary>
        /// This function will create or update the specified component data.
        /// </summary>
        /// <param name="item">ResourceComponent (json) - Component data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceComponent item)
        {
            try
            {
                serviceResponse = await _resourceComponentService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    // Get the item
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Component (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceComponent");
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

        // POST api/<ResourceComponentsController>
        /// <summary>
        /// This function will update all components data.
        /// </summary>
        /// <param name="items">List - ResourceComponent (json) - All components configuration data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceComponent> items)
        {
            try
            {
                serviceResponse = await _resourceComponentService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Components added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceComponent");
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
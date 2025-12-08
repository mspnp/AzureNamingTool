using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource delimiters.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceDelimitersController : ControllerBase
    {
        private readonly IResourceDelimiterService _resourceDelimiterService;
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDelimitersController"/> class.
        /// </summary>
        /// <param name="resourceDelimiterService">The resource delimiter service.</param>
        /// <param name="adminLogService">The admin log service.</param>
        public ResourceDelimitersController(
            IResourceDelimiterService resourceDelimiterService,
            IAdminLogService adminLogService)
        {
            _resourceDelimiterService = resourceDelimiterService ?? throw new ArgumentNullException(nameof(resourceDelimiterService));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Response for controller functions
        /// </summary>
        ServiceResponse serviceResponse = new();

        // GET api/<ResourceDelimitersController>
        /// <summary>
        /// This function will return the delimiters data.
        /// </summary>
        /// <param name="admin">bool - All/Only-enabled delimiters flag</param>
        /// <returns>json - Current delimiters data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                serviceResponse = await _resourceDelimiterService.GetItemsAsync(admin);
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

        // GET api/<ResourceDelimitersController>/5
        /// <summary>
        /// This function will return the specifed resource delimiter data.
        /// </summary>
        /// <param name="id">int - Resource Delimiter id</param>
        /// <returns>json - Resource delimiter data</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await _resourceDelimiterService.GetItemAsync(id);
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

        // POST api/<ResourceDelimitersController>
        /// <summary>
        /// This function will create/update the specified delimiter data.
        /// </summary>
        /// <param name="item">ResourceDelimiter (json) - Delimiter data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceDelimiter item)
        {
            try
            {
                serviceResponse = await _resourceDelimiterService.PostItemAsync(item);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Delimiter (" + item.Name + ") added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceDelimiter");
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

        // POST api/<resourcedelimitersController>
        /// <summary>
        /// This function will update all delimiters data.
        /// </summary>
        /// <param name="items">List - ResourceDelimiter (json) - All delimiters data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceDelimiter> items)
        {
            try
            {
                serviceResponse = await _resourceDelimiterService.PostConfigAsync(items);
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Delimiters added/updated." });
                    CacheHelper.InvalidateCacheObject("ResourceDelimiter");
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

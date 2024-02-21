using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using AzureNamingTool.Services;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing resource types.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceTypesController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();

        // GET: api/<ResourceTypesController>
        /// <summary>
        /// This function will return the resource types data. 
        /// </summary>
        /// <param name="admin">bool - Indicates if the user is an admin (optional)</param>
        /// <returns>json - Current resource types data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                // Get list of items
                serviceResponse = await ResourceTypeService.GetItems(admin);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await ResourceTypeService.GetItem(id);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceType> items)
        {
            try
            {
                serviceResponse = await ResourceTypeService.PostConfig(items);
                if (serviceResponse.Success)
                {
                    AdminLogService.PostItem(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Types updated." });
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
                serviceResponse = await ResourceTypeService.UpdateTypeComponents(operation, componentid);
                if (serviceResponse.Success)
                {
                    AdminLogService.PostItem(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Resource Types updated." });
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }
    }
}
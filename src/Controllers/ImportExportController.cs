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
    /// Controller for importing and exporting configuration data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ImportExportController : ControllerBase
    {
        private readonly IImportExportService _importExportService;
        private readonly IAdminLogService _adminLogService;

        public ImportExportController(
            IImportExportService importExportService,
            IAdminLogService adminLogService)
        {
            _importExportService = importExportService;
            _adminLogService = adminLogService;
        }

        /// <summary>
        /// Response for controller functions
        /// </summary>
        ServiceResponse serviceResponse = new();

        // GET: api/<ImportExportController>
        /// <summary>
        /// This function will export the current configuration data (all components) as a single JSON file. 
        /// </summary>
        /// <param name="includeAdmin">Flag indicating whether to include admin data in the export</param>
        /// <returns>JSON configuration file</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ExportConfiguration(bool includeAdmin = false)
        {
            try
            {
                serviceResponse = await _importExportService.ExportConfigAsync(includeAdmin);
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

        // POST api/<ImportExportController>
        /// <summary>
        /// This function will import the provided configuration data (all components). This will overwrite the existing configuration data. 
        /// </summary>
        /// <param name="configdata">Tool configuration file in JSON format</param>
        /// <returns>Flag indicating whether the import was successful or not</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ImportConfiguration([FromBody] ConfigurationData configdata)
        {
            try
            {
                serviceResponse = await _importExportService.PostConfigAsync(configdata);
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
    }
}

#pragma warning restore CS1591
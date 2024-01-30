using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AzureNamingTool.Services;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for importing and exporting configuration data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ImportExportController : ControllerBase
    {
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
                serviceResponse = await ImportExportService.ExportConfig(includeAdmin);
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
                serviceResponse = await ImportExportService.PostConfig(configdata);
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
    }
}

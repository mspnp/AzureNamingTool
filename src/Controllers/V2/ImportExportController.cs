using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Threading.Tasks;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// V2 API controller for importing and exporting configuration data with modern REST practices.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ImportExportController : ControllerBase
    {
        private readonly IImportExportService _importExportService;
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        public ImportExportController(
            IImportExportService importExportService,
            IAdminLogService adminLogService)
        {
            _importExportService = importExportService ?? throw new ArgumentNullException(nameof(importExportService));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Exports the current configuration data (all components) as a single JSON file.
        /// </summary>
        /// <param name="includeAdmin">Flag indicating whether to include admin data in the export</param>
        /// <returns>JSON configuration file</returns>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ConfigurationData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportConfiguration(bool includeAdmin = false)
        {
            try
            {
                var serviceResponse = await _importExportService.ExportConfigAsync(includeAdmin);
                
                var response = new ApiResponse<object>
                {
                    Success = serviceResponse.Success,
                    Data = serviceResponse.ResponseObject,
                    Metadata = new ApiMetadata
                    {
                        CorrelationId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow,
                        Version = "2.0"
                    }
                };

                if (!serviceResponse.Success)
                {
                    response.Error = new ApiError
                    {
                        Code = "EXPORT_FAILED",
                        Message = "Failed to export configuration data"
                    };
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while exporting configuration: {ex.Message}",
                    "ImportExportController.ExportConfiguration"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Imports the provided configuration data (all components). This will overwrite the existing configuration data.
        /// </summary>
        /// <param name="configdata">Tool configuration file in JSON format</param>
        /// <returns>Flag indicating whether the import was successful or not</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportConfiguration([FromBody] ConfigurationData configdata)
        {
            try
            {
                if (configdata == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "INVALID_REQUEST",
                            Message = "Configuration data cannot be null"
                        },
                        Metadata = new ApiMetadata
                        {
                            CorrelationId = HttpContext.TraceIdentifier,
                            Timestamp = DateTime.UtcNow,
                            Version = "2.0"
                        }
                    });
                }

                var serviceResponse = await _importExportService.PostConfigAsync(configdata);
                
                var response = new ApiResponse<object>
                {
                    Success = serviceResponse.Success,
                    Data = serviceResponse.ResponseObject,
                    Metadata = new ApiMetadata
                    {
                        CorrelationId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow,
                        Version = "2.0"
                    }
                };

                if (!serviceResponse.Success)
                {
                    response.Error = new ApiError
                    {
                        Code = "IMPORT_FAILED",
                        Message = "Failed to import configuration data"
                    };
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while importing configuration: {ex.Message}",
                    "ImportExportController.ImportConfiguration"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }
    }
}

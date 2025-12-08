# Script to generate V2 controllers for Azure Naming Tool
# This creates standardized V2 controllers with ApiResponse wrapper and proper error handling

$controllers = @(
    @{Name="ResourceDelimiters"; Service="IResourceDelimiterService"; Model="ResourceDelimiter"; HasDelete=$false},
    @{Name="ResourceEnvironments"; Service="IResourceEnvironmentService"; Model="ResourceEnvironment"; HasDelete=$true},
    @{Name="ResourceFunctions"; Service="IResourceFunctionService"; Model="ResourceFunction"; HasDelete=$true},
    @{Name="ResourceLocations"; Service="IResourceLocationService"; Model="ResourceLocation"; HasDelete=$false},
    @{Name="ResourceOrgs"; Service="IResourceOrgService"; Model="ResourceOrg"; HasDelete=$true},
    @{Name="ResourceProjAppSvcs"; Service="IResourceProjAppSvcService"; Model="ResourceProjAppSvc"; HasDelete=$true},
    @{Name="ResourceUnitDepts"; Service="IResourceUnitDeptService"; Model="ResourceUnitDept"; HasDelete=$true},
    @{Name="ResourceComponents"; Service="IResourceComponentService"; Model="ResourceComponent"; HasDelete=$false},
    @{Name="CustomComponents"; Service="ICustomComponentService"; Model="CustomComponent"; HasDelete=$true},
    @{Name="Policy"; Service="IPolicyService"; Model="Policy"; HasDelete=$false}
)

$template = @'
using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// V2 API controller for managing {DISPLAY_NAME} with modern REST practices.
    /// </summary>
    [Route("api/v{{version:apiVersion}}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class {CONTROLLER_NAME}Controller : ControllerBase
    {
        private readonly {SERVICE_INTERFACE} _{SERVICE_VAR};
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="{CONTROLLER_NAME}Controller"/> class.
        /// </summary>
        public {CONTROLLER_NAME}Controller(
            {SERVICE_INTERFACE} {SERVICE_PARAM},
            IAdminLogService adminLogService)
        {
            _{SERVICE_VAR} = {SERVICE_PARAM} ?? throw new ArgumentNullException(nameof({SERVICE_PARAM}));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Gets all {DISPLAY_NAME_LOWER}.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<{MODEL_NAME}>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var serviceResponse = await _{SERVICE_VAR}.GetItemsAsync();
                
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
                        Code = "FETCH_FAILED",
                        Message = "Failed to retrieve {DISPLAY_NAME_LOWER}"
                    };
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving {DISPLAY_NAME_LOWER}: {{ex.Message}}",
                    "{CONTROLLER_NAME}Controller.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Gets a specific {MODEL_NAME_LOWER} by ID.
        /// </summary>
        [HttpGet("{{id:int}}")]
        [ProducesResponseType(typeof(ApiResponse<{MODEL_NAME}>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var serviceResponse = await _{SERVICE_VAR}.GetItemAsync(id);
                
                if (!serviceResponse.Success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "NOT_FOUND",
                            Message = $"{MODEL_NAME} with ID {{id}} not found"
                        },
                        Metadata = new ApiMetadata
                        {
                            CorrelationId = HttpContext.TraceIdentifier,
                            Timestamp = DateTime.UtcNow,
                            Version = "2.0"
                        }
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = serviceResponse.ResponseObject,
                    Metadata = new ApiMetadata
                    {
                        CorrelationId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow,
                        Version = "2.0"
                    }
                });
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving the {MODEL_NAME_LOWER}: {{ex.Message}}",
                    "{CONTROLLER_NAME}Controller.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Updates a {MODEL_NAME_LOWER}.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<{MODEL_NAME}>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] {MODEL_NAME} item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "INVALID_REQUEST",
                            Message = "Request body cannot be null"
                        },
                        Metadata = new ApiMetadata
                        {
                            CorrelationId = HttpContext.TraceIdentifier,
                            Timestamp = DateTime.UtcNow,
                            Version = "2.0"
                        }
                    });
                }

                var serviceResponse = await _{SERVICE_VAR}.PostItemAsync(item);
                
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
                        Code = "UPDATE_FAILED",
                        Message = "Failed to update {MODEL_NAME_LOWER}"
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
                    $"An unexpected error occurred while updating the {MODEL_NAME_LOWER}: {{ex.Message}}",
                    "{CONTROLLER_NAME}Controller.Post"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

{DELETE_METHOD}

        /// <summary>
        /// Updates all {DISPLAY_NAME_LOWER}.
        /// </summary>
        [HttpPost("PostConfig")]
        [ProducesResponseType(typeof(ApiResponse<List<{MODEL_NAME}>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostConfig([FromBody] List<{MODEL_NAME}> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "INVALID_REQUEST",
                            Message = "Request body cannot be null or empty"
                        },
                        Metadata = new ApiMetadata
                        {
                            CorrelationId = HttpContext.TraceIdentifier,
                            Timestamp = DateTime.UtcNow,
                            Version = "2.0"
                        }
                    });
                }

                var serviceResponse = await _{SERVICE_VAR}.PostConfigAsync(items);
                
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
                        Code = "UPDATE_FAILED",
                        Message = "Failed to update {DISPLAY_NAME_LOWER} configuration"
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
                    $"An unexpected error occurred while updating the {DISPLAY_NAME_LOWER} configuration: {{ex.Message}}",
                    "{CONTROLLER_NAME}Controller.PostConfig"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }
    }
}
'@

$deleteMethod = @'
        /// <summary>
        /// Deletes a {MODEL_NAME_LOWER}.
        /// </summary>
        [HttpDelete("{{id:int}}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var serviceResponse = await _{SERVICE_VAR}.DeleteItemAsync(id);
                
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
                        Code = "DELETE_FAILED",
                        Message = $"Failed to delete {MODEL_NAME_LOWER} with ID {{id}}"
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
                    $"An unexpected error occurred while deleting the {MODEL_NAME_LOWER}: {{ex.Message}}",
                    "{CONTROLLER_NAME}Controller.Delete"
                );
                response.Error!.InnerError = new ApiInnerError {{ Code = ex.GetType().Name }};
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

'@

foreach ($ctrl in $controllers) {
    $controllerName = $ctrl.Name
    $serviceName = $ctrl.Service
    $modelName = $ctrl.Model
    $hasDelete = $ctrl.HasDelete
    
    # Generate variable names
    $serviceVar = $serviceName.Substring(1)  # Remove 'I' prefix
    $serviceVar = $serviceVar.Substring(0,1).ToLower() + $serviceVar.Substring(1)  # camelCase
    $serviceParam = $serviceVar
    
    $displayName = $controllerName -creplace '([A-Z])', ' $1'
    $displayName = $displayName.Trim()
    $displayNameLower = $displayName.ToLower()
    $modelNameLower = $modelName.ToLower()
    
    # Generate content
    $content = $template
    $content = $content -replace '\{CONTROLLER_NAME\}', $controllerName
    $content = $content -replace '\{SERVICE_INTERFACE\}', $serviceName
    $content = $content -replace '\{SERVICE_VAR\}', $serviceVar
    $content = $content -replace '\{SERVICE_PARAM\}', $serviceParam
    $content = $content -replace '\{MODEL_NAME\}', $modelName
    $content = $content -replace '\{DISPLAY_NAME\}', $displayName
    $content = $content -replace '\{DISPLAY_NAME_LOWER\}', $displayNameLower
    $content = $content -replace '\{MODEL_NAME_LOWER\}', $modelNameLower
    
    if ($hasDelete) {
        $deleteContent = $deleteMethod
        $deleteContent = $deleteContent -replace '\{MODEL_NAME_LOWER\}', $modelNameLower
        $deleteContent = $deleteContent -replace '\{SERVICE_VAR\}', $serviceVar
        $deleteContent = $deleteContent -replace '\{CONTROLLER_NAME\}', $controllerName
        $deleteContent = $deleteContent -replace '\{MODEL_NAME\}', $modelName
        $content = $content -replace '\{DELETE_METHOD\}', $deleteContent
    } else {
        $content = $content -replace '\{DELETE_METHOD\}', ''
    }
    
    # Write file
    $outputPath = "c:\Projects\AzureNamingTool-DEV\src\Controllers\V2\$($controllerName)Controller.cs"
    $content | Out-File -FilePath $outputPath -Encoding UTF8 -Force
    
    Write-Host "Created $outputPath" -ForegroundColor Green
}

Write-Host "`nAll V2 controllers created successfully!" -ForegroundColor Cyan

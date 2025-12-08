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
    /// V2 API controller for managing Resource Functions with modern REST practices.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceFunctionsController : ControllerBase
    {
        private readonly IResourceFunctionService _resourceFunctionService;
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFunctionsController"/> class.
        /// </summary>
        public ResourceFunctionsController(
            IResourceFunctionService resourceFunctionService,
            IAdminLogService adminLogService)
        {
            _resourceFunctionService = resourceFunctionService ?? throw new ArgumentNullException(nameof(resourceFunctionService));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Gets all resource functions.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ResourceFunction>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var serviceResponse = await _resourceFunctionService.GetItemsAsync();
                
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
                        Message = "Failed to retrieve resource functions"
                    };
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving resource functions: {{ex.Message}}",
                    "ResourceFunctionsController.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Gets a specific resourcefunction by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceFunction>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var serviceResponse = await _resourceFunctionService.GetItemAsync(id);
                
                if (!serviceResponse.Success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "NOT_FOUND",
                            Message = $"ResourceFunction with ID {{id}} not found"
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
                    $"An unexpected error occurred while retrieving the resourcefunction: {{ex.Message}}",
                    "ResourceFunctionsController.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Updates a resourcefunction.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceFunction>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ResourceFunction item)
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

                var serviceResponse = await _resourceFunctionService.PostItemAsync(item);
                
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
                        Message = "Failed to update resourcefunction"
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
                    $"An unexpected error occurred while updating the resourcefunction: {{ex.Message}}",
                    "ResourceFunctionsController.Post"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Deletes a resourcefunction.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var serviceResponse = await _resourceFunctionService.DeleteItemAsync(id);
                
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
                        Message = $"Failed to delete resourcefunction with ID {{id}}"
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
                    $"An unexpected error occurred while deleting the resourcefunction: {{ex.Message}}",
                    "ResourceFunctionsController.Delete"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }


        /// <summary>
        /// Updates all resource functions.
        /// </summary>
        [HttpPost("PostConfig")]
        [ProducesResponseType(typeof(ApiResponse<List<ResourceFunction>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceFunction> items)
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

                var serviceResponse = await _resourceFunctionService.PostConfigAsync(items);
                
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
                        Message = "Failed to update resource functions configuration"
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
                    $"An unexpected error occurred while updating the resource functions configuration: {{ex.Message}}",
                    "ResourceFunctionsController.PostConfig"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }
    }
}

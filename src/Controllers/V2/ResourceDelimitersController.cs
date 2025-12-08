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
    /// V2 API controller for managing Resource Delimiters with modern REST practices.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
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
        public ResourceDelimitersController(
            IResourceDelimiterService resourceDelimiterService,
            IAdminLogService adminLogService)
        {
            _resourceDelimiterService = resourceDelimiterService ?? throw new ArgumentNullException(nameof(resourceDelimiterService));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Gets all resource delimiters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ResourceDelimiter>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                var serviceResponse = await _resourceDelimiterService.GetItemsAsync(admin);
                
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
                        Message = "Failed to retrieve resource delimiters"
                    };
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving resource delimiters: {{ex.Message}}",
                    "ResourceDelimitersController.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Gets a specific resourcedelimiter by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceDelimiter>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var serviceResponse = await _resourceDelimiterService.GetItemAsync(id);
                
                if (!serviceResponse.Success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = new ApiError
                        {
                            Code = "NOT_FOUND",
                            Message = $"ResourceDelimiter with ID {{id}} not found"
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
                    $"An unexpected error occurred while retrieving the resourcedelimiter: {{ex.Message}}",
                    "ResourceDelimitersController.Get"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Updates a resourcedelimiter.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceDelimiter>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ResourceDelimiter item)
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

                var serviceResponse = await _resourceDelimiterService.PostItemAsync(item);
                
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
                        Message = "Failed to update resourcedelimiter"
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
                    $"An unexpected error occurred while updating the resourcedelimiter: {{ex.Message}}",
                    "ResourceDelimitersController.Post"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }



        /// <summary>
        /// Updates all resource delimiters.
        /// </summary>
        [HttpPost("PostConfig")]
        [ProducesResponseType(typeof(ApiResponse<List<ResourceDelimiter>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceDelimiter> items)
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

                var serviceResponse = await _resourceDelimiterService.PostConfigAsync(items);
                
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
                        Message = "Failed to update resource delimiters configuration"
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
                    $"An unexpected error occurred while updating the resource delimiters configuration: {{ex.Message}}",
                    "ResourceDelimitersController.PostConfig"
                );
                response.Error!.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }
    }
}

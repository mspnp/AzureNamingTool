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
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// Controller for managing resource types (API v2.0).
    /// This version uses standardized ApiResponse wrapper for consistent response format.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceTypesController : ControllerBase
    {
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceTypesController(
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        }

        /// <summary>
        /// Get all resource types. 
        /// </summary>
        /// <param name="admin">bool - Indicates if the user is an admin (optional)</param>
        /// <returns>Standardized API response with resource types data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Models.ResourceType>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                var serviceResponse = await _resourceTypeService.GetItemsAsync(admin);
                
                if (serviceResponse.Success)
                {
                    var data = serviceResponse.ResponseObject as List<Models.ResourceType>;
                    var response = ApiResponse<List<Models.ResourceType>>.SuccessResponse(
                        data ?? new List<Models.ResourceType>(),
                        "Resource types retrieved successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "RESOURCE_TYPES_FETCH_FAILED",
                        serviceResponse.ResponseObject?.ToString() ?? "Failed to retrieve resource types"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - Get ResourceTypes failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving resource types: {ex.Message}",
                    "ResourceTypesController.Get"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Get a specific resource type by ID.
        /// </summary>
        /// <param name="id">int - Resource Type id</param>
        /// <returns>Standardized API response with resource type data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Models.ResourceType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var serviceResponse = await _resourceTypeService.GetItemAsync(id);
                
                if (serviceResponse.Success)
                {
                    var data = serviceResponse.ResponseObject as Models.ResourceType;
                    
                    if (data == null)
                    {
                        var notFoundResponse = ApiResponse<object>.ErrorResponse(
                            "RESOURCE_TYPE_NOT_FOUND",
                            $"Resource type with ID {id} not found",
                            $"ResourceType/{id}"
                        );
                        notFoundResponse.Metadata.CorrelationId = GetCorrelationId();
                        return NotFound(notFoundResponse);
                    }
                    
                    var response = ApiResponse<Models.ResourceType>.SuccessResponse(
                        data,
                        "Resource type retrieved successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "RESOURCE_TYPE_FETCH_FAILED",
                        serviceResponse.ResponseObject?.ToString() ?? "Failed to retrieve resource type"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - Get ResourceType {id} failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while retrieving resource type: {ex.Message}",
                    $"ResourceTypesController.Get/{id}"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Update resource type configuration (bulk update).
        /// </summary>
        /// <param name="items">List of resource types to update</param>
        /// <returns>Standardized API response with success status</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] List<Models.ResourceType> items)
        {
            try
            {
                var serviceResponse = await _resourceTypeService.PostConfigAsync(items);
                
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = "V2 API - Resource Types updated successfully" 
                    });
                    
                    var response = ApiResponse<string>.SuccessResponse(
                        "Configuration updated successfully",
                        $"Updated {items.Count} resource type(s)"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "RESOURCE_TYPES_UPDATE_FAILED",
                        serviceResponse.ResponseObject?.ToString() ?? "Failed to update resource types"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - Update ResourceTypes failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while updating resource types: {ex.Message}",
                    "ResourceTypesController.Post"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }
    }
}

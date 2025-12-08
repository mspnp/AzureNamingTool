#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Threading.Tasks;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// Controller for handling resource naming requests (API v2.0).
    /// This version uses standardized ApiResponse wrapper with enhanced error handling.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceNamingRequestsController : ControllerBase
    {
        private readonly IResourceNamingRequestService _resourceNamingRequestService;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAzureValidationService _azureValidationService;
        private readonly ConflictResolutionService _conflictResolutionService;

        public ResourceNamingRequestsController(
            IResourceNamingRequestService resourceNamingRequestService,
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService,
            IHttpContextAccessor httpContextAccessor,
            IAzureValidationService azureValidationService,
            ConflictResolutionService conflictResolutionService)
        {
            _resourceNamingRequestService = resourceNamingRequestService;
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
            _httpContextAccessor = httpContextAccessor;
            _azureValidationService = azureValidationService;
            _conflictResolutionService = conflictResolutionService;
        }

        private string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        }

        /// <summary>
        /// Performs Azure tenant validation and conflict resolution on a generated name
        /// </summary>
        private async Task<(string finalName, AzureValidationMetadata? validationMetadata)> ValidateAndResolveConflictAsync(
            string generatedName,
            string resourceType,
            bool skipValidation = false)
        {
            // Check if validation is enabled and not skipped
            if (skipValidation || !await _azureValidationService.IsValidationEnabledAsync())
            {
                return (generatedName, null);
            }

            try
            {
                // Get Azure validation settings
                var settings = await _azureValidationService.GetSettingsAsync();

                // Validate the generated name
                var validation = await _azureValidationService.ValidateNameAsync(generatedName, resourceType);
                validation.ValidationPerformed = true;

                // If name doesn't exist in Azure, we're done
                if (!validation.ExistsInAzure)
                {
                    return (generatedName, validation);
                }

                // Name exists - apply conflict resolution strategy
                var resolution = await _conflictResolutionService.ResolveConflictAsync(
                    generatedName,
                    resourceType,
                    settings);

                // Update validation metadata with resolution details
                validation.OriginalName = resolution.OriginalName;
                validation.IncrementAttempts = resolution.Attempts;
                validation.ValidationWarning = resolution.Warning ?? resolution.ErrorMessage;

                // Return the resolved name (or original if resolution failed)
                return (resolution.FinalName, validation);
            }
            catch (Exception ex)
            {
                _adminLogService.PostItemAsync(new AdminLogMessage()
                {
                    Title = "WARNING",
                    Message = $"Azure validation failed: {ex.Message}. Continuing with original name."
                }).GetAwaiter().GetResult();

                // On error, return original name with error metadata
                return (generatedName, new AzureValidationMetadata
                {
                    ValidationPerformed = true,
                    ValidationWarning = $"Validation error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Generate a resource type name with full component definition.
        /// This function requires complete definition for all components.
        /// It is recommended to use the RequestName API function for simplified name generation.
        /// </summary>
        /// <param name="request">ResourceNameRequestWithComponents (json) - Complete resource name request data with all components</param>
        /// <returns>Standardized API response with name generation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ResourceNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
        {
            try
            {
                if (request == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ResourceNameRequestWithComponents"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                var resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameWithComponentsAsync(request);
                
                if (resourceNameRequestResponse.Success)
                {
                    var response = ApiResponse<ResourceNameResponse>.SuccessResponse(
                        resourceNameRequestResponse,
                        "Resource name generated successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "NAME_GENERATION_FAILED",
                        resourceNameRequestResponse.Message ?? "Failed to generate resource name",
                        "RequestNameWithComponents"
                    );
                    
                    // Include validation details if available
                    if (!string.IsNullOrEmpty(resourceNameRequestResponse.Message))
                    {
                        response.Error!.Details = new System.Collections.Generic.List<ApiError>
                        {
                            new ApiError
                            {
                                Code = "VALIDATION_ERROR",
                                Message = resourceNameRequestResponse.Message
                            }
                        };
                    }
                    
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - RequestNameWithComponents failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while generating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.RequestNameWithComponents"
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
        /// Generate a resource type name using simplified data format.
        /// This is the recommended method for name generation as it uses a simpler request structure.
        /// </summary>
        /// <param name="request">ResourceNameRequest (json) - Simplified resource name request data</param>
        /// <returns>Standardized API response with name generation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ResourceNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
        {
            try
            {
                if (request == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ResourceNameRequest"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                request.CreatedBy = "API-V2";
                var resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameAsync(request);
                
                if (resourceNameRequestResponse.Success)
                {
                    // Perform Azure tenant validation and conflict resolution
                    var (finalName, validationMetadata) = await ValidateAndResolveConflictAsync(
                        resourceNameRequestResponse.ResourceName,
                        request.ResourceType,
                        skipValidation: false);

                    // Update the response with the final name and validation metadata
                    resourceNameRequestResponse.ResourceName = finalName;
                    resourceNameRequestResponse.ValidationMetadata = validationMetadata;

                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = $"V2 API - Generated name: {finalName}" +
                                  (validationMetadata?.ValidationPerformed == true ? " (Azure validated)" : "")
                    });
                    
                    var response = ApiResponse<ResourceNameResponse>.SuccessResponse(
                        resourceNameRequestResponse,
                        "Resource name generated successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "NAME_GENERATION_FAILED",
                        resourceNameRequestResponse.Message ?? "Failed to generate resource name",
                        "RequestName"
                    );
                    
                    // Include validation details if available
                    if (!string.IsNullOrEmpty(resourceNameRequestResponse.Message))
                    {
                        response.Error!.Details = new System.Collections.Generic.List<ApiError>
                        {
                            new ApiError
                            {
                                Code = "VALIDATION_ERROR",
                                Message = resourceNameRequestResponse.Message
                            }
                        };
                    }
                    
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - RequestName failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while generating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.RequestName"
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
        /// Validate a resource name against the regex pattern for the specified resource type.
        /// NOTE: This function validates using the resource type regex only, not the full tool configuration.
        /// Use the RequestName function to validate against the complete tool configuration.
        /// </summary>
        /// <param name="validateNameRequest">ValidateNameRequest (json) - Name validation request data</param>
        /// <returns>Standardized API response with validation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ValidateNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateName([FromBody] ValidateNameRequest validateNameRequest)
        {
            try
            {
                if (validateNameRequest == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ValidateNameRequest"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (string.IsNullOrEmpty(validateNameRequest.Name))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_NAME",
                        "Name is required for validation",
                        "ValidateNameRequest.Name"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (string.IsNullOrEmpty(validateNameRequest.ResourceType))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_RESOURCE_TYPE",
                        "ResourceType is required for validation",
                        "ValidateNameRequest.ResourceType"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                var serviceResponse = await _resourceTypeService.ValidateResourceTypeNameAsync(validateNameRequest);
                
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        var validateNameResponse = (ValidateNameResponse)serviceResponse.ResponseObject!;
                        var response = ApiResponse<ValidateNameResponse>.SuccessResponse(
                            validateNameResponse,
                            validateNameResponse.Valid 
                                ? "Resource name is valid" 
                                : "Resource name validation failed"
                        );
                        response.Metadata.CorrelationId = GetCorrelationId();
                        return Ok(response);
                    }
                    else
                    {
                        var response = ApiResponse<object>.ErrorResponse(
                            "VALIDATION_RESULT_NULL",
                            "There was a problem validating the name - no result returned",
                            "ValidateName"
                        );
                        response.Metadata.CorrelationId = GetCorrelationId();
                        return BadRequest(response);
                    }
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "VALIDATION_FAILED",
                        serviceResponse.ResponseObject?.ToString() ?? "There was a problem validating the name",
                        "ValidateName"
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
                    Message = $"V2 API - ValidateName failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while validating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.ValidateName"
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
        /// Generate resource names for multiple resource types in a single request.
        /// Processes each resource type independently and returns detailed success/failure information.
        /// </summary>
        /// <param name="request">BulkResourceNameRequest (json) - Bulk name generation request with shared components and resource types</param>
        /// <returns>Standardized API response with bulk name generation results</returns>
        /// <remarks>
        /// This endpoint allows you to generate names for multiple resource types in a single API call.
        /// 
        /// **Key Features:**
        /// - Process multiple resource types with shared component values
        /// - Continue processing on errors (default: true)
        /// - Per-resource-type component overrides
        /// - Validate-only mode (test names without persisting)
        /// - Detailed success/failure reporting for each resource
        /// 
        /// **Response Status Codes:**
        /// - 200 OK: All resource names generated successfully
        /// - 207 Multi-Status: Partial success (some succeeded, some failed)
        /// - 400 Bad Request: Validation error or all failed
        /// 
        /// **Sample Request:**
        /// 
        ///     POST /api/v2/ResourceNamingRequests/GenerateBulk
        ///     {
        ///       "resourceTypes": ["rg", "vnet", "nsg"],
        ///       "resourceLocation": "use",
        ///       "resourceInstance": "001",
        ///       "resourceTypeOverrides": {
        ///         "vnet": {
        ///           "resourceInstance": "002"
        ///         }
        ///       },
        ///       "continueOnError": true,
        ///       "validateOnly": false,
        ///       "createdBy": "MyApp"
        ///     }
        /// 
        /// **Sample Response (200 OK):**
        /// 
        ///     {
        ///       "success": true,
        ///       "data": {
        ///         "results": [
        ///           {
        ///             "resourceType": "rg",
        ///             "success": true,
        ///             "resourceName": "rg-use-001"
        ///           },
        ///           {
        ///             "resourceType": "vnet",
        ///             "success": true,
        ///             "resourceName": "vnet-use-002"
        ///           },
        ///           {
        ///             "resourceType": "nsg",
        ///             "success": true,
        ///             "resourceName": "nsg-use-001"
        ///           }
        ///         ],
        ///         "success": true,
        ///         "message": "Successfully generated 3 resource name(s)",
        ///         "totalRequested": 3,
        ///         "successCount": 3,
        ///         "failureCount": 0
        ///       }
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<BulkResourceNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BulkResourceNameResponse>), StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateBulk([FromBody] BulkResourceNameRequest request)
        {
            try
            {
                if (request == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "BulkResourceNameRequest"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                // Validate resource types list
                if (request.ResourceTypes == null || request.ResourceTypes.Count == 0)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "ResourceTypes list cannot be null or empty",
                        "BulkResourceNameRequest.ResourceTypes"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                var bulkResponse = new BulkResourceNameResponse
                {
                    TotalRequested = request.ResourceTypes.Count,
                    ProcessedAt = DateTime.UtcNow
                };

                // Process each resource type
                foreach (var resourceTypeShortName in request.ResourceTypes)
                {
                    var result = new BulkResourceNameResult
                    {
                        ResourceType = resourceTypeShortName
                    };

                    try
                    {
                        // Build individual request for this resource type
                        var individualRequest = new ResourceNameRequest
                        {
                            ResourceType = resourceTypeShortName,
                            ResourceEnvironment = request.ResourceEnvironment,
                            ResourceFunction = request.ResourceFunction,
                            ResourceInstance = request.ResourceInstance,
                            ResourceLocation = request.ResourceLocation,
                            ResourceOrg = request.ResourceOrg,
                            ResourceProjAppSvc = request.ResourceProjAppSvc,
                            ResourceUnitDept = request.ResourceUnitDept,
                            CustomComponents = request.CustomComponents,
                            CreatedBy = request.CreatedBy
                        };

                        // Apply resource type overrides if specified
                        if (request.ResourceTypeOverrides != null && 
                            request.ResourceTypeOverrides.TryGetValue(resourceTypeShortName, out var overrides))
                        {
                            if (!string.IsNullOrEmpty(overrides.ResourceEnvironment))
                                individualRequest.ResourceEnvironment = overrides.ResourceEnvironment;
                            if (!string.IsNullOrEmpty(overrides.ResourceFunction))
                                individualRequest.ResourceFunction = overrides.ResourceFunction;
                            if (!string.IsNullOrEmpty(overrides.ResourceInstance))
                                individualRequest.ResourceInstance = overrides.ResourceInstance;
                            if (!string.IsNullOrEmpty(overrides.ResourceLocation))
                                individualRequest.ResourceLocation = overrides.ResourceLocation;
                            if (!string.IsNullOrEmpty(overrides.ResourceOrg))
                                individualRequest.ResourceOrg = overrides.ResourceOrg;
                            if (!string.IsNullOrEmpty(overrides.ResourceProjAppSvc))
                                individualRequest.ResourceProjAppSvc = overrides.ResourceProjAppSvc;
                            if (!string.IsNullOrEmpty(overrides.ResourceUnitDept))
                                individualRequest.ResourceUnitDept = overrides.ResourceUnitDept;
                            if (overrides.CustomComponents != null && overrides.CustomComponents.Count > 0)
                                individualRequest.CustomComponents = overrides.CustomComponents;
                        }

                        // Generate the name
                        var nameResponse = await _resourceNamingRequestService.RequestNameAsync(individualRequest);

                        if (nameResponse.Success)
                        {
                            // Perform Azure validation and conflict resolution
                            var (finalName, validationMetadata) = await ValidateAndResolveConflictAsync(
                                nameResponse.ResourceName,
                                resourceTypeShortName,
                                skipValidation: false);

                            result.Success = true;
                            result.ResourceName = finalName;
                            result.ValidationMetadata = validationMetadata;
                            
                            if (!request.ValidateOnly)
                            {
                                result.ResourceNameDetails = nameResponse.ResourceNameDetails;
                            }

                            bulkResponse.SuccessCount++;
                        }
                        else
                        {
                            result.Success = false;
                            result.ErrorMessage = nameResponse.Message;
                            bulkResponse.FailureCount++;

                            // If not continue on error, stop processing
                            if (!request.ContinueOnError)
                            {
                                bulkResponse.Results.Add(result);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Exception occurred: {ex.Message}";
                        bulkResponse.FailureCount++;

                        await _adminLogService.PostItemAsync(new AdminLogMessage()
                        {
                            Title = "ERROR",
                            Message = $"V2 API - GenerateBulk failed for resource type '{resourceTypeShortName}': {ex.Message}"
                        });

                        // If not continue on error, stop processing
                        if (!request.ContinueOnError)
                        {
                            bulkResponse.Results.Add(result);
                            break;
                        }
                    }

                    bulkResponse.Results.Add(result);
                }

                // Set overall success and message
                bulkResponse.Success = bulkResponse.FailureCount == 0;
                
                if (bulkResponse.Success)
                {
                    bulkResponse.Message = $"Successfully generated {bulkResponse.SuccessCount} resource name(s)";
                }
                else if (bulkResponse.SuccessCount > 0)
                {
                    bulkResponse.Message = $"Partially successful: {bulkResponse.SuccessCount} succeeded, {bulkResponse.FailureCount} failed";
                }
                else
                {
                    bulkResponse.Message = $"All {bulkResponse.FailureCount} resource name generation(s) failed";
                }

                // Return appropriate status code
                if (bulkResponse.Success)
                {
                    var response = ApiResponse<BulkResourceNameResponse>.SuccessResponse(
                        bulkResponse,
                        bulkResponse.Message
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else if (bulkResponse.SuccessCount > 0)
                {
                    // Partial success - return 207 Multi-Status
                    var response = ApiResponse<BulkResourceNameResponse>.SuccessResponse(
                        bulkResponse,
                        bulkResponse.Message
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return StatusCode(207, response);
                }
                else
                {
                    // Complete failure - return 400
                    var response = ApiResponse<BulkResourceNameResponse>.SuccessResponse(
                        bulkResponse,
                        bulkResponse.Message
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
                    Message = $"V2 API - GenerateBulk failed: {ex.Message}"
                });

                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred during bulk name generation: {ex.Message}",
                    "ResourceNamingRequestsController.GenerateBulk"
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

#pragma warning restore CS1591

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// Controller for managing Admin settings (API v2.0).
    /// This version uses proper HTTP status codes and standardized ApiResponse wrapper.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAdminLogService _adminLogService;
        private readonly IGeneratedNamesService _generatedNamesService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SiteConfiguration config = ConfigurationHelper.GetConfigurationData();

        public AdminController(
            IAdminService adminService,
            IAdminLogService adminLogService,
            IGeneratedNamesService generatedNamesService,
            IHttpContextAccessor httpContextAccessor)
        {
            _adminService = adminService;
            _adminLogService = adminLogService;
            _generatedNamesService = generatedNamesService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        }

        /// <summary>
        /// Update the Global Admin Password. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <param name="password">string - New Global Admin Password</param>
        /// <returns>Standardized API response with success status</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePassword(
            [BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, 
            [FromBody] string password)
        {
            try
            {
                if (string.IsNullOrEmpty(adminpassword))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_ADMIN_PASSWORD",
                        "You must provide the Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (adminpassword != GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INCORRECT_ADMIN_PASSWORD",
                        "Incorrect Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Unauthorized(response);
                }

                var serviceResponse = await _adminService.UpdatePasswordAsync(password);
                
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = "V2 API - Global Admin Password updated successfully" 
                    });
                    
                    var response = ApiResponse<string>.SuccessResponse(
                        "Password updated successfully",
                        "Global Admin Password has been updated"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "PASSWORD_UPDATE_FAILED",
                        "There was a problem updating the password",
                        "AdminController.UpdatePassword"
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
                    Message = $"V2 API - UpdatePassword failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while updating password: {ex.Message}",
                    "AdminController.UpdatePassword"
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
        /// Update the Full Access API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <param name="apikey">string - New Full Access API Key</param>
        /// <returns>Standardized API response with success status</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAPIKey(
            [BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, 
            [FromBody] string apikey)
        {
            try
            {
                if (string.IsNullOrEmpty(adminpassword))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_ADMIN_PASSWORD",
                        "You must provide the Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (adminpassword != GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INCORRECT_ADMIN_PASSWORD",
                        "Incorrect Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Unauthorized(response);
                }

                var serviceResponse = await _adminService.UpdateAPIKeyAsync(apikey, "fullaccess");
                
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = "V2 API - Full Access API Key updated successfully" 
                    });
                    
                    var response = ApiResponse<string>.SuccessResponse(
                        "API Key updated successfully",
                        "Full Access API Key has been updated"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "API_KEY_UPDATE_FAILED",
                        "There was a problem updating the Full Access API Key",
                        "AdminController.UpdateAPIKey"
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
                    Message = $"V2 API - UpdateAPIKey failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while updating API key: {ex.Message}",
                    "AdminController.UpdateAPIKey"
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
        /// Generate a new Full Access API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>Standardized API response with new API key</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateAPIKey(
            [BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (string.IsNullOrEmpty(adminpassword))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_ADMIN_PASSWORD",
                        "You must provide the Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (adminpassword != GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INCORRECT_ADMIN_PASSWORD",
                        "Incorrect Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Unauthorized(response);
                }

                var serviceResponse = await _adminService.GenerateAPIKeyAsync("fullaccess");
                
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = "V2 API - New Full Access API Key generated successfully" 
                    });
                    
                    var apiKey = serviceResponse.ResponseObject?.ToString() ?? string.Empty;
                    var response = ApiResponse<string>.SuccessResponse(
                        apiKey,
                        "New Full Access API Key generated successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "API_KEY_GENERATION_FAILED",
                        "There was a problem generating the new API key",
                        "AdminController.GenerateAPIKey"
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
                    Message = $"V2 API - GenerateAPIKey failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while generating API key: {ex.Message}",
                    "AdminController.GenerateAPIKey"
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
        /// Clear all generated names from the cache. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>Standardized API response with success status</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearGeneratedNames(
            [BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (string.IsNullOrEmpty(adminpassword))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_ADMIN_PASSWORD",
                        "You must provide the Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (adminpassword != GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INCORRECT_ADMIN_PASSWORD",
                        "Incorrect Global Admin Password",
                        "AdminPassword header"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Unauthorized(response);
                }

                var serviceResponse = await _generatedNamesService.DeleteAllItemsAsync();
                
                if (serviceResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = "V2 API - Generated Names cleared successfully" 
                    });
                    
                    var response = ApiResponse<string>.SuccessResponse(
                        "Generated names cleared successfully",
                        "All generated names have been removed from the cache"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "CLEAR_NAMES_FAILED",
                        "There was a problem clearing the generated names",
                        "AdminController.ClearGeneratedNames"
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
                    Message = $"V2 API - ClearGeneratedNames failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while clearing generated names: {ex.Message}",
                    "AdminController.ClearGeneratedNames"
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

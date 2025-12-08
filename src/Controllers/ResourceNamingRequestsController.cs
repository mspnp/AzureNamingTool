#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using System.Threading;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for handling resource naming requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceNamingRequestsController : ControllerBase
    {
        private readonly IResourceNamingRequestService _resourceNamingRequestService;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;

        public ResourceNamingRequestsController(
            IResourceNamingRequestService resourceNamingRequestService,
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService)
        {
            _resourceNamingRequestService = resourceNamingRequestService;
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
        }

        private ServiceResponse serviceResponse = new();
        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will generate a resource type name for specified component values. This function requires full definition for all components. It is recommended to use the RequestName API function for name generation.
        /// </summary>
        /// <param name="request">ResourceNameRequestWithComponents (json) - Resource Name Request data</param>
        /// <returns>string - Name generation response</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ResourceNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResourceNameResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
        {
            try
            {
                ResourceNameResponse resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameWithComponentsAsync(request);
                if (resourceNameRequestResponse.Success)
                {
                    return Ok(resourceNameRequestResponse);
                }
                else
                {
                    return BadRequest(resourceNameRequestResponse);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }

        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will generate a resource type name for specified component values, using a simple data format.
        /// </summary>
        /// <param name="request">ResourceNameRequest (json) - Resource Name Request data</param>
        /// <returns>string - Name generation response</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ResourceNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResourceNameResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
        {
            try
            {
                request.CreatedBy = "API";
                ResourceNameResponse resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameAsync(request);
                if (resourceNameRequestResponse.Success)
                {
                    return Ok(resourceNameRequestResponse);
                }
                else
                {
                    return BadRequest(resourceNameRequestResponse);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }

        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will validate the name for the specified resource type. NOTE: This function does not validate using the tool configuration, only the regex for the specified resource type. Use the RequestName function to validate using the tool configuration.
        /// </summary>
        /// <param name="validateNameRequest">ValidateNameRequest (json) - Validate Name Request data</param>
        /// <returns>ValidateNameResponse - Name validation response</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ValidateNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateName([FromBody] ValidateNameRequest validateNameRequest)
        {
            try
            {
                // Get the current delimiter
                serviceResponse = await _resourceTypeService.ValidateResourceTypeNameAsync(validateNameRequest);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        ValidateNameResponse validateNameResponse = (ValidateNameResponse)serviceResponse.ResponseObject!;
                        return Ok(validateNameResponse);
                    }
                    else
                    {
                        return BadRequest("There was a problem validating the name.");
                    }
                }
                else
                {
                    return BadRequest("There was a problem validating the name.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }
    }
}

#pragma warning restore CS1591
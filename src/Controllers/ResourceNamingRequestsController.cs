using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
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
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for handling resource naming requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceNamingRequestsController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will generate a resource type name for specified component values. This function requires full definition for all components. It is recommended to use the RequestName API function for name generation.
        /// </summary>
        /// <param name="request">ResourceNameRequestWithComponents (json) - Resource Name Request data</param>
        /// <returns>string - Name generation response</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
        {
            try
            {
                ResourceNameResponse resourceNameRequestResponse = await ResourceNamingRequestService.RequestNameWithComponents(request);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
        {
            try
            {
                request.CreatedBy = "API";
                ResourceNameResponse resourceNameRequestResponse = await ResourceNamingRequestService.RequestName(request);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        public async Task<IActionResult> ValidateName([FromBody] ValidateNameRequest validateNameRequest)
        {
            try
            {
                // Get the current delimiter
                serviceResponse = await ResourceTypeService.ValidateResourceTypeName(validateNameRequest);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }
    }
}

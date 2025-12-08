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
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AzureNamingTool.Controllers
{
    /// <summary>
    /// Controller for managing Admin settings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAdminLogService _adminLogService;
        private readonly IGeneratedNamesService _generatedNamesService;
        private readonly SiteConfiguration config = ConfigurationHelper.GetConfigurationData();

        public AdminController(
            IAdminService adminService,
            IAdminLogService adminLogService,
            IGeneratedNamesService generatedNamesService)
        {
            _adminService = adminService;
            _adminLogService = adminLogService;
            _generatedNamesService = generatedNamesService;
        }

        private ServiceResponse serviceResponse = new();

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the Global Admin Password. 
        /// </summary>
        /// <param name="password">string - New Global Admin Password</param>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>string - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePassword([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string password)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.UpdatePasswordAsync(password);
                        return (serviceResponse.Success ? Ok("SUCCESS") : Ok("FAILURE - There was a problem updating the password."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the Full Access API Key. 
        /// </summary>
        /// <param name="apikey">string - New Full Access API Key</param>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>dttring - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string apikey)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.UpdateAPIKeyAsync(apikey, "fullaccess");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem updating the Full Access API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will generate a new Full Access API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>string - Successful update / Generated Full Access API Key</returns>

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.GenerateAPIKeyAsync("fullaccess");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem generating the Full Access API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }




        // POST api/<AdminController>
        /// <summary>
        /// This function will update the Name Generation API Key. 
        /// </summary>
        /// <param name="apikey">string - Name Generation API Key</param>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>dttring - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateNameGenerationAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string apikey)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.UpdateAPIKeyAsync(apikey, "namegeneration");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem updating the Name Generation API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will generate a new Name Generation API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>string - Successful update / Generated Name Generation API Key</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateNameGenerationAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.GenerateAPIKeyAsync("namegeneration");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem generating the Name Generation API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the Read-Only API Key. 
        /// </summary>
        /// <param name="apikey">string - New Read-Only API Key</param>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>dttring - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateReadOnlyAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string apikey)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.UpdateAPIKeyAsync(apikey, "readonly");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem updating the Read-Only API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will generate a new Read-Only API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Global Admin Password</param>
        /// <returns>string - Successful update / Generated Read-Only API Key</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateReadOnlyAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminService.GenerateAPIKeyAsync("readonly");
                        return (serviceResponse.Success ? Ok("SUCCESS") : BadRequest(" - There was a problem generating the Read-Only API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function will return the admin log data.
        /// </summary>
        /// <returns>json - Current admin log data</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetAdminLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminLogService.GetItemsAsync();
                        if (serviceResponse.Success)
                        {
                            return Ok(serviceResponse.ResponseObject);
                        }
                        else
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function will purge the admin log data.
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PurgeAdminLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _adminLogService.DeleteAllItemsAsync();
                        if (serviceResponse.Success)
                        {
                            return Ok(serviceResponse.ResponseObject);
                        }
                        else
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function will return the generated names data.
        /// </summary>
        /// <returns>json - Current generated names data</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetGeneratedNamesLog()
        {
            try
            {
                serviceResponse = await _generatedNamesService.GetItemsAsync();
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
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // GET api/<AdminController>/GetGeneratedName/5
        /// <summary>
        /// This function will return the generated names data by ID.
        /// </summary>
        /// <param name="id">int - Generated Name id</param>
        /// <returns>json - Current generated name data by ID</returns>
        [HttpGet]
        [Route("[action]/{id}")]
        public async Task<IActionResult> GetGeneratedName(int id)
        {
            try
            {
                serviceResponse = await _generatedNamesService.GetItemAsync(id);
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
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // DELETE api/<AdminController>/DeleteGeneratedName/5
        /// <summary>
        /// This function will delete the generated names data by ID.
        /// </summary>
        /// <param name="adminpassword">string - Admin password</param>
        /// <param name="id">int - Generated Name id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete]
        [Route("[action]/{id}")]
        public async Task<IActionResult> DeleteGeneratedName([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, int id)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        // Get the item details
                        serviceResponse = await _generatedNamesService.GetItemAsync(id);
                        if (serviceResponse.Success)
                        {
                            GeneratedName item = (GeneratedName)serviceResponse.ResponseObject!;
                            serviceResponse = await _generatedNamesService.DeleteItemAsync(id);
                            if (serviceResponse.Success)
                            {
                                await _adminLogService.PostItemAsync(new AdminLogMessage() { Source = "API", Title = "INFORMATION", Message = "Generated Name (" + item.ResourceName + ") deleted." });
                                CacheHelper.InvalidateCacheObject("GeneratedName");
                                return Ok("Generated Name (" + item.ResourceName + ") deleted.");
                            }
                            else
                            {
                                return BadRequest(serviceResponse.ResponseObject);
                            }
                        }
                        else
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function will purge the generated names data.
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PurgeGeneratedNamesLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        serviceResponse = await _generatedNamesService.DeleteAllItemsAsync();
                        if (serviceResponse.Success)
                        {
                            return Ok(serviceResponse.ResponseObject);
                        }
                        else
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function resets the site configuration to the default values.
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResetSiteConfiguration([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (GeneralHelper.IsNotNull(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword!, config.SALTKey!))
                    {
                        if (ConfigurationHelper.ResetSiteConfiguration())
                        {
                            return Ok("Site configuration reset suceeded!");
                        }
                        else
                        {
                            return BadRequest("Site configuration reset failed!");
                        }
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Global Admin Password.");
                    }
                }
                else
                {
                    return Ok("FAILURE - You must provide the Global Admin Password.");
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }
    }
}

#pragma warning restore CS1591
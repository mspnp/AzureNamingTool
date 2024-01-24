using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the admin settings.
    /// </summary>
    public class AdminService
    {
        private static readonly SiteConfiguration config = ConfigurationHelper.GetConfigurationData();

        /// <summary>
        /// Updates the user password.
        /// </summary>
        /// <param name="password">The new password.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> UpdatePassword(string password)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                if (ValidationHelper.ValidatePassword(password))
                {
                    config.AdminPassword = GeneralHelper.EncryptString(password, config.SALTKey!);
                    await ConfigurationHelper.UpdateSettings(config);
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "The password does not meet the security requirements.";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Generates a new API key based on the specified type.
        /// </summary>
        /// <param name="type">The type of API key to generate (fullaccess or readonly).</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GenerateAPIKey(string type)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Set the new api key
                Guid guid = Guid.NewGuid();
                switch (type)
                {
                    case "fullaccess":
                        config.APIKey = GeneralHelper.EncryptString(guid.ToString(), config.SALTKey!);
                        break;
                    case "readonly":
                        config.ReadOnlyAPIKey = GeneralHelper.EncryptString(guid.ToString(), config.SALTKey!);
                        break;
                }
                await ConfigurationHelper.UpdateSettings(config);
                serviceResponse.ResponseObject = guid.ToString();
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Updates the API key based on the specified type.
        /// </summary>
        /// <param name="apikey">The new API key.</param>
        /// <param name="type">The type of API key to update (fullaccess or readonly).</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> UpdateAPIKey(string apikey, string type)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                switch (type)
                {
                    case "fullaccess":
                        config.APIKey = GeneralHelper.EncryptString(apikey, config.SALTKey!);
                        break;

                    case "readonly":
                        config.ReadOnlyAPIKey = GeneralHelper.EncryptString(apikey, config.SALTKey!);
                        break;

                }
                await ConfigurationHelper.UpdateSettings(config);
                serviceResponse.ResponseObject = apikey;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Updates the identity header name.
        /// </summary>
        /// <param name="identityheadername">The new identity header name.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> UpdateIdentityHeaderName(string identityheadername)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                config.IdentityHeaderName = GeneralHelper.EncryptString(identityheadername, config.SALTKey!);
                await ConfigurationHelper.UpdateSettings(config);
                serviceResponse.ResponseObject = identityheadername;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}
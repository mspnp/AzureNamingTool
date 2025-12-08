using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AzureNamingTool.Attributes
{
    /// <summary>
    /// Attribute to validate API key for authorization.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "APIKey";

        /// <summary>
        /// Validates the API key for authorization.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        /// <param name="next">The action execution delegate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided!"
                };
                return;
            }

            var config = ConfigurationHelper.GetConfigurationData();
            if (GeneralHelper.IsNotNull(config))
            {
                // Determine if the request is read-only
                if (context.HttpContext.Request.Method == "GET")
                {
                    if ((!GeneralHelper.DecryptString(config.APIKey!, config.SALTKey!).Equals(extractedApiKey)) && (!GeneralHelper.DecryptString(config.ReadOnlyAPIKey!, config.SALTKey!).Equals(extractedApiKey)) && (!GeneralHelper.DecryptString(config.NameGenerationAPIKey!, config.SALTKey!).Equals(extractedApiKey)))
                    {
                        context.Result = new ContentResult()
                        {
                            StatusCode = 401,
                            Content = "Api Key is not valid!"
                        };
                        return;
                    }
                }
                else
                {
                    // Request is a POST. Make sure the provided API Key is for full access
                    // Check if it is a name generation request (V1 or V2 API)
                    if (context.HttpContext.Request.Path.Value.StartsWith("/api/ResourceNamingRequests/") || 
                        context.HttpContext.Request.Path.Value.Contains("/ResourceNamingRequests/"))
                    {
                        if ((!GeneralHelper.DecryptString(config.APIKey!, config.SALTKey!).Equals(extractedApiKey)) && (!GeneralHelper.DecryptString(config.NameGenerationAPIKey!, config.SALTKey!).Equals(extractedApiKey)))
                        {
                            context.Result = new ContentResult()
                            {
                                StatusCode = 401,
                                Content = "Api Key is not valid!"
                            };
                            return;
                        }
                    }
                    else
                    {
                        if (!GeneralHelper.DecryptString(config.APIKey!, config.SALTKey!).Equals(extractedApiKey))
                        {
                            context.Result = new ContentResult()
                            {
                                StatusCode = 401,
                                Content = "Full Access Api Key is not valid!"
                            };
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }
}

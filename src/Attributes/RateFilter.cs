using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Helpers;

namespace AzureNamingTool.Attributes
{
    /// <summary>
    /// Represents a rate filter attribute that sets the minimum data rate for request and response.
    /// </summary>
    public class RateFilter : Attribute, IResourceFilter
    {
        /// <summary>
        /// Executes the filter before a resource is executed.
        /// </summary>
        /// <param name="context">The resource executing context.</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            try
            {
                var minRequestRateFeature = context.HttpContext.Features.Get<IHttpMinRequestBodyDataRateFeature>();
                var minResponseRateFeature = context.HttpContext.Features.Get<IHttpMinResponseDataRateFeature>();
                //Default Bytes/s = 240, Default TimeOut = 5s

                if (GeneralHelper.IsNotNull(minRequestRateFeature))
                {
                    minRequestRateFeature.MinDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                }

                if (GeneralHelper.IsNotNull(minResponseRateFeature))
                {
                    minResponseRateFeature.MinDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        /// <summary>
        /// Executes the filter after a resource is executed.
        /// </summary>
        /// <param name="context">The resource executed context.</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}

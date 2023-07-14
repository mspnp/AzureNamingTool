using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Helpers;

namespace AzureNamingTool.Attributes
{
    public class RateFilter : Attribute, IResourceFilter
    {
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

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}

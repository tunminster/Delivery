using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Delivery.Azure.Library.WebApi.Telemetry;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Azure.Library.WebApi.Filters
{
    /// <summary>
    ///  Logs the request and response data to application insights.
    /// </summary>
    public class ApiLoggingFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            SetRequestData(context);
            await next();
        }
        
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            SetRequestData(context);

            await next();
            await context.HttpContext.TrackRequestAsync();
        }

        private static void SetRequestData(FilterContext context)
        {
            var request = context.HttpContext.Request;
            
            if (context.HttpContext.Items.ContainsKey(HttpRequestTracer.StartTime))
            {
                request.SetUserEmail();
                return;
            }

            context.HttpContext.Items[HttpRequestTracer.StartTime] = DateTimeOffset.UtcNow;
            request.SetCorrelationId();
            request.SetUserEmail();
        }

    }
}
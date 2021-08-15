using System.Threading.Tasks;
using Delivery.Azure.Library.Core;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Azure.Library.WebApi.Filters
{
    /// <summary>
    ///     Sets a correlation id to the request and response if it doesn't exist
    /// </summary>
    public class CorrelationIdFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var correlationId = context.HttpContext.Request.GetCorrelationId();
            if (!string.IsNullOrEmpty(correlationId))
            {
                context.HttpContext.Response.Headers[HttpHeaders.CorrelationId] = correlationId;
            }

            await next();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var existingCorrelationId = context.HttpContext.Request.GetCorrelationId();
            if (string.IsNullOrEmpty(existingCorrelationId))
            {
                context.HttpContext.Request.SetCorrelationId();
            }

            context.HttpContext.TraceIdentifier = context.HttpContext.Request.GetCorrelationId();

            await next();
        }
    }
}
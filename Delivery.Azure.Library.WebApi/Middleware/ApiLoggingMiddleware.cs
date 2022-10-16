using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Delivery.Azure.Library.WebApi.Telemetry;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.WebApi.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate next;

        public ApiLoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                SetRequestData(context);
                await next(context);
            }
            finally
            {
                await context.TrackRequestAsync();
            }
        }

        private static void SetRequestData(HttpContext context)
        {
            var request = context.Request;
            if (context.Items.ContainsKey(HttpRequestTracer.StartTime))
            {
                return;
            }

            context.Items[HttpRequestTracer.StartTime] = DateTimeOffset.UtcNow;
            request.SetCorrelationId();
            //request.SetUserEmail();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.WebApi.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.WebApi.Telemetry
{
    public static class HttpRequestTracer
    {
        public const string StartTime = "ApiLoggingFilterAttribute_StartTime";

        /// <summary>
        ///     Tracks the request based on the values configured in <see cref="ApiLoggingFilterAttribute" />
        /// </summary>
        /// <param name="httpContext">The request http context</param>
        /// <remarks>Requires that <see cref="ApiLoggingFilterAttribute" /> has been used in order to calculate timings</remarks>
        public static async Task TrackRequestAsync(this HttpContext httpContext)
        {
            var response = httpContext.Response;
            var elapsedTime = CalculateElapsedTime(httpContext);

            var telemetry = httpContext.RequestServices.GetRequiredService<IApplicationInsightsTelemetry>();
            await telemetry.TrackRequestAsync(response, elapsedTime, httpContext.GetTelemetryProperties());
            var routeFriendlyName = string.Join(".", httpContext.Request.RouteValues.Where(p => p.Key.ToLowerInvariant() == "controller" || p.Key.ToLowerInvariant() == "action").Select(p => p.Value).Reverse());

            var isInMemoryTest = IsInMemoryTest(httpContext.RequestServices);
            var metricSuffix = isInMemoryTest ? " (localhost)" : string.Empty;
            telemetry.TrackMetric($"{routeFriendlyName} Processing Time{metricSuffix}", (int) elapsedTime.TotalMilliseconds, httpContext.GetTelemetryProperties());
        }

        private static TimeSpan CalculateElapsedTime(HttpContext httpContext)
        {
            if (httpContext.Items.ContainsKey(StartTime) && httpContext.Items[StartTime] is DateTimeOffset startTime)
            {
                return DateTimeOffset.UtcNow.Subtract(startTime);
            }

            return TimeSpan.Zero;
        }

        private static bool IsInMemoryTest(IServiceProvider serviceProvider)
        {
            var useInMemory = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<bool>("Test_Use_In_Memory", defaultValue: false);
            return useInMemory;
        }
    }
}
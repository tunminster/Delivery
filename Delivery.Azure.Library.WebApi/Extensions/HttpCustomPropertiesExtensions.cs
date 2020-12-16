using System.Collections.Generic;
using Delivery.Azure.Library.Sharding.Exceptions;
using Delivery.Azure.Library.Sharding.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.WebApi.Extensions
{
    public static class HttpCustomPropertiesExtensions
    {
        public static Dictionary<string, string> GetTelemetryProperties(this HttpContext httpContext)
        {
            var customProperties = new Dictionary<string, string>();
            var correlationId = httpContext.Request.GetCorrelationId();
            var ring = httpContext.Request.GetRing();
            try
            {
                var shard = httpContext.Request.GetShard();
                customProperties.Add(CustomProperties.Shard, shard.Key);
            }
            catch (MultipleShardsFoundException)
            {
                // no shards in header
            }
            catch (ShardNotFoundException)
            {
                // no shards in header
            }

            customProperties.Add(CustomProperties.CorrelationId, correlationId);
            customProperties.Add(CustomProperties.Ring, ring?.ToString() ?? "Unknown");
            return customProperties;
        }
    }
}
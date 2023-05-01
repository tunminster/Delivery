using System.Collections.Generic;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Telemetry.Constants;

namespace Delivery.Azure.Library.Telemetry.Extensions
{
    public static class ExecutingRequestContextExtensions
    {
        public static Dictionary<string, string> GetTelemetryProperties(
            this ExecutingRequestContext executingRequestContext)
        {
            var customProperties = new Dictionary<string, string>
            {
                {CustomProperties.Shard, executingRequestContext.ShardKey},
                {CustomProperties.CorrelationId, executingRequestContext.CorrelationId},
                {CustomProperties.Ring, executingRequestContext.Ring.ToString()}
            };
            return customProperties;
        }
        public static Dictionary<string, string> GetTelemetryProperties(
            this AnonymousExecutingRequestContext anonymousExecutingRequestContext)
        {
            if (anonymousExecutingRequestContext is ExecutingRequestContext executingRequestContext)
            {
                return executingRequestContext.GetTelemetryProperties();
            }

            return new()
            {
                {CustomProperties.CorrelationId, anonymousExecutingRequestContext.CorrelationId},
                {CustomProperties.Ring, anonymousExecutingRequestContext.Ring.ToString()}
            };
        }
    }
}
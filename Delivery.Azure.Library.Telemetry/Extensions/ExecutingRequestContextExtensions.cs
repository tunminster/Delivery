using System.Collections.Generic;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Telemetry.Constants;

namespace Delivery.Azure.Library.Telemetry.Extensions
{
    public static class ExecutingRequestContextExtensions
    {
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
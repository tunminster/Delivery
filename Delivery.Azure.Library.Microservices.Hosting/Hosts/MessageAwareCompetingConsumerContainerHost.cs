using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Serialization;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Microservices.Hosting.Hosts
{
    public abstract class MessageAwareCompetingConsumerContainerHost<TMessage, TState> : CompetingConsumerContainerHost<TState>
        where TMessage : class
        where TState : Enum
    {
        protected MessageAwareCompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
        {
        }

        /// <summary>
        ///     Provides the core extension point for containers to insert their message-processing logic
        /// </summary>
        /// <param name="message">The deserialized message body for the business logic to use</param>
        /// <param name="processingState">The state in which the message was last processed, if at all</param>
        /// <param name="correlationId">The single correlation id which all telemetry should forward</param>
        /// <param name="stopwatch">The running stopwatch which will be used to benchmark message processing time</param>
        /// <param name="telemetryContextProperties">The telemetry properties which should be forwarded to the logger</param>
        protected abstract Task ProcessMessageAsync(TMessage message, TState processingState, string correlationId, ApplicationInsightsStopwatch stopwatch, Dictionary<string, string> telemetryContextProperties);

        protected override async Task ProcessMessageAsync(Message message, string correlationId, ApplicationInsightsStopwatch stopwatch, Dictionary<string, string> telemetryContextProperties)
        {
            var processingState = message.GetMessageProcessingState<TState>();
            var deserializedMessage = message.Deserialize<TMessage>();

            await ProcessMessageAsync(deserializedMessage, processingState, correlationId, stopwatch, telemetryContextProperties);
        }
    }
}
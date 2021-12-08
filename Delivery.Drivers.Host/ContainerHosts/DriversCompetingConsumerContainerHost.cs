using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.ServiceBus;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Domain.Contracts.Enums;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Drivers.Host.ContainerHosts
{
    public class DriversCompetingConsumerContainerHost : CompetingConsumerContainerHost<OrderMessageProcessingStates>
    {
        public DriversCompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override string QueueOrTopicName => ServiceProvider.GetRequiredService<IConfigurationProvider>()
            .GetSetting("Topic_Name").ToLowerInvariant();

        protected override async Task ProcessMessageAsync(Message message, string correlationId,
            ApplicationInsightsStopwatch stopwatch,
            Dictionary<string, string> telemetryContextProperties)
        {
            var processingState = message.GetMessageProcessingState<OrderMessageProcessingStates>();
            var ring = message.GetRing();

            if (Ring != ring)
            {
                throw new InvalidOperationException(
                    $"The ring specified in the message {ring} does not match the ring which this {GetType().Name} can handle (ring: {Ring}). Properties: {telemetryContextProperties.Format()}");
            }

            if (!message.UserProperties.TryGetValue(UserProperties.MessageType, out var messageType))
            {
                throw new InvalidOperationException($"Message needs to have a {UserProperties.MessageType} header");
            }
        }
    }
}
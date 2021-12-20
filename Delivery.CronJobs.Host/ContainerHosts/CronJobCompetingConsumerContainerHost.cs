using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Domain.Contracts.Enums;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.CronJobs.Host.ContainerHosts
{
    public class CronJobCompetingConsumerContainerHost : CompetingConsumerContainerHost<OrderMessageProcessingStates>
    {
        public CronJobCompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override string QueueOrTopicName => ServiceProvider.GetRequiredService<IConfigurationProvider>()
        .GetSetting("Topic_Name").ToLowerInvariant();

        protected override async Task ProcessMessageAsync(Message message, string correlationId, ApplicationInsightsStopwatch stopwatch,
            Dictionary<string, string> telemetryContextProperties)
        {
            //await Task.CompletedTask();
        }
    }
}
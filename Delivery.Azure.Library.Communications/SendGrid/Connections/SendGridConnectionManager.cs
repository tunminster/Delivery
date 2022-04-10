using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Interfaces;
using Delivery.Azure.Library.Communications.SendGrid.Providers;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Reliability;

namespace Delivery.Azure.Library.Communications.SendGrid.Connections
{
    public class SendGridConnectionManager : ConnectionManager<SendGridConnection>, ISendGridConnectionManager
    {
        public SendGridConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override int PartitionCount => ServiceProvider.GetRequiredService<IConfigurationProvider>()
            .GetSettingOrDefault("SendGrid_ConnectionManager_PartitionCount", defaultValue: 0);

        public override DependencyType DependencyType => DependencyType.Email;
        public override ExternalDependency ExternalDependency => ExternalDependency.SendGrid;
        protected override async Task<SendGridConnection> CreateConnectionAsync(IConnectionMetadata connectionMetadata)
        {
            var sendGridConfigurationDefinition = new SendGridEmailNotificationConfigurationDefinition(ServiceProvider);

            var sendGridClientOptions = new SendGridClientOptions
            {
                ApiKey = sendGridConfigurationDefinition.GetEmailNotificationAuthKeySecret(),
                ReliabilitySettings = new ReliabilitySettings(
                    sendGridConfigurationDefinition.DefaultMaximumNumberOfRetries,
                    sendGridConfigurationDefinition.DefaultMinimumBackOff,
                    sendGridConfigurationDefinition.DefaultMaximumBackOff,
                    sendGridConfigurationDefinition.DefaultDeltaBackOff)
            };

            var sendGridClient = new SendGridClient(sendGridClientOptions);

            await Task.CompletedTask;
            return new SendGridConnection(connectionMetadata, sendGridClient);
        }
    }
}
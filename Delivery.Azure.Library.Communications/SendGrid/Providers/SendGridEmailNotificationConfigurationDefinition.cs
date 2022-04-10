using System;
using Microsoft.Extensions.DependencyInjection;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;

namespace Delivery.Azure.Library.Communications.SendGrid.Providers
{
    public class SendGridEmailNotificationConfigurationDefinition
    {
        private readonly IServiceProvider serviceProvider;

        private IConfigurationProvider ConfigurationProvider =>
            serviceProvider.GetRequiredService<IConfigurationProvider>();

        public SendGridEmailNotificationConfigurationDefinition(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string GetEmailNotificationAuthKeySecret()
        {
            return ConfigurationProvider.GetSetting("SendGrid-Api-Key");
        }

        public string SenderEmail => ConfigurationProvider.GetSetting("SendGrid-SenderEmail");

        public int DefaultMaximumNumberOfRetries =>
            ConfigurationProvider.GetSettingOrDefault("SendGrid-DefaultMaximumNumberOfRetries", 2);

        public TimeSpan DefaultMinimumBackOff =>
            ConfigurationProvider.GetSettingOrDefault("SendGrid-DefaultMaximumNumberOfRetries",
                TimeSpan.FromSeconds(value: 1));

        public TimeSpan DefaultMaximumBackOff => ConfigurationProvider.GetSettingOrDefault(
            "SendGrid-DefaultMaximumNumberOfRetries",
            TimeSpan.FromSeconds(value: 1));

        public TimeSpan DefaultDeltaBackOff =>
            ConfigurationProvider.GetSettingOrDefault("SendGrid-DefaultMaximumNumberOfRetries",
                TimeSpan.FromSeconds(value: 1));
    }
}
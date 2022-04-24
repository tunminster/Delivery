using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Driver.Domain.Configurations;

public class DriverEmailNotificationConfigurationDefinition : ConfigurationDefinition
{
    private readonly IServiceProvider serviceProvider;

    public DriverEmailNotificationConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public string SenderEmail => ConfigurationProvider.GetSetting("SendGrid-SenderEmail");

    public string ReplyEmail => ConfigurationProvider.GetSetting("SendGrid-ReplyEmail");


}
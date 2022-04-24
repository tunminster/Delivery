using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Configurations;

namespace Delivery.Driver.Domain.Factories
{
    public class DriverEmailTemplateFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DriverEmailTemplateFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public SendGridEmailCreationContract Create(DriverEmailTemplateModel driverEmailTemplateModel)
        {
            var driverEmailNotificationConfiguration =
                new DriverEmailNotificationConfigurationDefinition(serviceProvider);
            var sendGridEmailCreationContract = new SendGridEmailCreationContract
            {
                Template = new EmailTemplateContract
                {
                    TemplateId = driverEmailTemplateModel.TemplateId,
                    DynamicTemplateData = driverEmailTemplateModel.DynamicTemplateData
                },
                InitiatedOn = DateTimeOffset.UtcNow,
                NotificationSendAt = DateTimeOffset.UtcNow,
                BusinessDomainType = driverEmailTemplateModel.BusinessDomainType.ToString(),
                EmailSender = new EmailContract
                {
                    EmailAddress = driverEmailNotificationConfiguration.SenderEmail,
                    Name = "Ragibull Driver Partner"
                },
                EmailReplyTo = new EmailContract
                {
                    EmailAddress = driverEmailNotificationConfiguration.ReplyEmail,
                    Name = "No-Reply"
                },
                Categories = new List<string>
                {
                    driverEmailTemplateModel.BusinessDomainType.ToString(), "email_notification"
                },
                Subject = driverEmailTemplateModel.Subject,
                NotificationUniqueId = driverEmailTemplateModel.NotificationUniqueId
            };
            return sendGridEmailCreationContract;
        }
    }
}
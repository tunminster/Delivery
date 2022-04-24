using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Communications.SendGrid.Services.EmailService;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.Models;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOnBoardingEmail;
using Delivery.Driver.Domain.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverOnBoardingEmail
{
    public record DriverOnBoardingEmailCommand(DriverOnBoardingEmailCreationContract DriverOnBoardingEmailCreationContract);
    public class DriverOnBoardingEmailCommandHandler : ICommandHandler<DriverOnBoardingEmailCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverOnBoardingEmailCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(DriverOnBoardingEmailCommand command)
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            var notificationUniqueId = $"{Guid.NewGuid()}-driver-on-boarding-link";
            var emailTemplateId = configurationProvider.GetSetting("SendGrid-Delivery-OnBoarding-EmailTemplateId");
            var emailTo = command.DriverOnBoardingEmailCreationContract.Email;
            var emailBcc = configurationProvider.GetSetting("SendGrid-On-Boarding-Bcc");

            var subject = "Driver on-boarding form";

            var driverOnBoardingDataModel = new DriverOnBoardingDataModel
            {
                Name = command.DriverOnBoardingEmailCreationContract.Name,
                Subject = subject,
                OnBoardingLink = command.DriverOnBoardingEmailCreationContract.OnBoardingLink
            };

            var driverEmailTemplateModel = new DriverEmailTemplateModel(BusinessDomainType.Driver, emailTemplateId,
                subject, driverOnBoardingDataModel, notificationUniqueId);

            var sendGridEmailCreationContract =
                new DriverEmailTemplateFactory(serviceProvider).Create(driverEmailTemplateModel);
            
            sendGridEmailCreationContract.EmailRecipients.Add(new EmailContract { EmailAddress = emailTo});
            sendGridEmailCreationContract.EmailAddressBccRecipients.Add(new EmailContract {EmailAddress = emailBcc});

            await new SendGridEmailService(serviceProvider, executingRequestContextAdapter).SendEmailAsync(
                sendGridEmailCreationContract);
        }
    }
}


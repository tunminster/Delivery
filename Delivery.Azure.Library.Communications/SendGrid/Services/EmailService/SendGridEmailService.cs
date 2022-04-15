using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.CommandHandlers;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.Enums;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Communications.SendGrid.Services.EmailService
{
    public class SendGridEmailService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public SendGridEmailService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task SendEmailAsync(SendGridEmailCreationContract sendGridEmailCreationContract)
        {
            var notificationUniqueId = sendGridEmailCreationContract.NotificationUniqueId;

            var sendGridEmailNotificationStatusContract = new SendGridEmailNotificationStatusContract
            {
                Status = NotificationStatus.MessagePublished,
                NotificationUniqueId = notificationUniqueId
            };
            
            var emailNotificationCommand = new EmailNotificationCommand(sendGridEmailCreationContract,
                sendGridEmailNotificationStatusContract);

            await new SendGridEmailNotificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(emailNotificationCommand);
        }
    }
}
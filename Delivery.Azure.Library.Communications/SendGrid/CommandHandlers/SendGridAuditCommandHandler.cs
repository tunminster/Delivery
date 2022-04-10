using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Core.Constants;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Services;

namespace Delivery.Azure.Library.Communications.SendGrid.CommandHandlers
{
    public record SendGridAuditCommand(EmailNotificationCommand EmailNotificationCommand);
    public class SendGridAuditCommandHandler
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public SendGridAuditCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task HandleAsync(SendGridAuditCommand command)
        {
            var cosmosDatabaseAccessor = await CosmosDatabaseAccessor.CreateAsync(serviceProvider,
                new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider,
                    Constants.CosmosDatabaseHnPlatformConnectionString));

            var platformCosmosDbService = new PlatformCosmosDbService(serviceProvider, executingRequestContextAdapter,
                cosmosDatabaseAccessor.CosmosClient,
                cosmosDatabaseAccessor.GetContainer(Constants.HnPlatform,
                    ManagementConstants.NotificationCollectionName));

            var emailNotificationCommand = command.EmailNotificationCommand;

            if (emailNotificationCommand == null)
            {
                throw new InvalidOperationException("Expected an email notification command.");
            }

            var attachments = new List<EmailAttachmentContract>();

            foreach (var emailAttachmentContract in command.EmailNotificationCommand.SendGridEmailCreationContract.EmailAttachments)
            {
                attachments.Add(emailAttachmentContract with {Stream = null});
            }
            
            var emailNotificationCreationContract = command.EmailNotificationCommand.SendGridEmailCreationContract with {EmailAttachments = attachments};

            var documentContract = new DocumentContract<SendGridAuditNotificationContract>
            {
                Id = Guid.NewGuid(),
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDate = DateTimeOffset.UtcNow,
                ValidFromDate = DateTimeOffset.UtcNow,
                ValidToDate = DateTimeOffset.UtcNow.AddYears(years: 100),
                PartitionKey = emailNotificationCreationContract.BusinessDomainType,
                Data = new List<SendGridAuditNotificationContract>
                {
                    new(emailNotificationCreationContract, command.EmailNotificationCommand.SendGridEmailNotificationStatusContract)
                }
            };

            await platformCosmosDbService
                .AddItemAsync<DocumentContract<SendGridAuditNotificationContract>, SendGridAuditNotificationContract>(
                    documentContract);
        }
    }
}
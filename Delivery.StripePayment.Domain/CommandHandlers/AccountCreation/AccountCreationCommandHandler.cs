using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Services;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.Models;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation
{
    public class AccountCreationCommandHandler :  ICommandHandler<AccountCreationCommand, StripeAccountCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeAccountCreationStatusContract> Handle(AccountCreationCommand command)
        {
            var stripeApiKey = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Stripe-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var options = new AccountCreateOptions
            {
                Type = command.StripeAccountCreationContract.StripeAccountType.ToString().ToLower(),
                Country = command.StripeAccountCreationContract.StripeCountryCode.ToString().ToLower(),
                Email = command.StripeAccountCreationContract.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = command.StripeAccountCreationContract.AccountPaymentOption,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = command.StripeAccountCreationContract.AccountTransferOption,
                    },
                },
            };
            
            var service = new AccountService();
            var account = await service.CreateAsync(options);

            var stripeAccountContract = new StripeAccountContract
            {
                AccountId = account.Id,
                Shard = executingRequestContextAdapter.GetShard().Key
            };

            var stripeDocumentAccount = new DocumentContract<StripeAccountContract>
            {
                Id = Guid.NewGuid(),
                PartitionKey = stripeAccountContract.Shard,
                Data = new List<StripeAccountContract> {stripeAccountContract},
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDate = DateTimeOffset.UtcNow,
                ValidFromDate = DateTimeOffset.UtcNow,
                Version = 1
            };
            
            //save into cosmosdb
            var cosmosDatabaseAccessor = await CosmosDatabaseAccessor.CreateAsync(serviceProvider, new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider, Constants.CosmosDatabaseHnPlatformConnectionString));
            var platformCosmosDbService = new PlatformCosmosDbService(serviceProvider, executingRequestContextAdapter, cosmosDatabaseAccessor.CosmosClient, cosmosDatabaseAccessor.GetContainer(Constants.HnPlatform, ContainerConstants.ContainerConstants.ConnectAccountCollectionName));
            await new PlatformCachedCosmosDbService(serviceProvider, executingRequestContextAdapter, platformCosmosDbService).AddItemAsync<DocumentContract<StripeAccountContract>, StripeAccountContract>(stripeDocumentAccount);
            
            // Create account link
            var stripeAccountLinkCreationContract = new StripeAccountLinkCreationContract
            {
                AccountId = account.Id,
                RefreshUrl = "https://www.example.com/reauth",
                ReturnUrl = "https://www.example.com/return"
            };

            var accountLinkCreationCommand = new AccountLinkCreationCommand(stripeAccountLinkCreationContract);
            var accountLinkCreationStatusContract =
                await new AccountLinkCreationCommandHandler(serviceProvider).Handle(accountLinkCreationCommand);
            
            
            var stripeAccountCreationStatusContract = new StripeAccountCreationStatusContract
            {
                AccountId = account.Id,
                AccountStatus = StripeAccountStatus.Created,
                OnBoardingAccountUrl = accountLinkCreationStatusContract.AccountLink
            };

            return stripeAccountCreationStatusContract;
        }
    }
}
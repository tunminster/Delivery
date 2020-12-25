using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Services;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation
{
    public class AccountCreationCommandHandler :  ICommandHandler<StripeAccountCreationContract, StripeAccountCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeAccountCreationStatusContract> Handle(StripeAccountCreationContract command)
        {
            var stripeApiKey = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Stripe-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var options = new AccountCreateOptions
            {
                Type = command.StripeAccountType.ToString().ToLower(),
                Country = command.StripeCountryCode.ToString().ToLower(),
                Email = command.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = command.AccountPaymentOption,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = command.AccountTransferOption,
                    },
                },
            };
            
            var service = new AccountService();
            var account = await service.CreateAsync(options);
            
            //Todo: save into cosmosdb
            var cosmosDatabaseAccessor = await CosmosDatabaseAccessor.CreateAsync(serviceProvider, new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider, Constants.CosmosDatabaseHnPlatformConnectionString));
            var platformCosmosDbService = new PlatformCosmosDbService(serviceProvider, executingRequestContextAdapter, cosmosDatabaseAccessor.CosmosClient, cosmosDatabaseAccessor.GetContainer(Constants.HnPlatform, ContainerConstants.ContainerConstants.ConnectAccountCollectionName));
                
            var stripeAccountCreationStatusContract = new StripeAccountCreationStatusContract
            {
                AccountId = account.Id,
                AccountStatus = StripeAccountStatus.Created
            };

            return stripeAccountCreationStatusContract;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Services;
using Delivery.Domain.Constants;
using Delivery.Domain.QueryHandlers;
using Delivery.StripePayment.Domain.Contracts.Models;
using Microsoft.Azure.Cosmos;

namespace Delivery.StripePayment.Domain.QueryHandlers.Stripe.AccountLinks
{
    public class AccountLinkGetQueryHandler : IQueryHandler<AccountLinkGetQuery, List<DocumentContract<StripeAccountContract>>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountLinkGetQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DocumentContract<StripeAccountContract>?>> Handle(AccountLinkGetQuery query)
        {
            var cosmosDatabaseAccessor = await CosmosDatabaseAccessor.CreateAsync(serviceProvider, new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider, Constants.CosmosDatabaseHnPlatformConnectionString));
            var platformCosmosDbService = new PlatformCosmosDbService(serviceProvider, executingRequestContextAdapter, cosmosDatabaseAccessor.CosmosClient, cosmosDatabaseAccessor.GetContainer(Constants.HnPlatform, ContainerConstants.ContainerConstants.ConnectAccountCollectionName));

            var queryDefinition =
                new QueryDefinition(
                    $"SELECT * FROM c WHERE c.partitionKey = {query.PartitionKey} and c.email = {query.Email} ");

            var stripeAccountContracts =
                await new PlatformCachedCosmosDbService(serviceProvider, executingRequestContextAdapter,
                    platformCosmosDbService).GetItemsAsync <
                DocumentContract<StripeAccountContract>>(queryDefinition);

            return stripeAccountContracts.ToList();
        }
    }
}
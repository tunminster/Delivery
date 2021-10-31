using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Services;
using Delivery.Domain.Constants;
using Delivery.Domain.Contracts.V1.RestContracts.TaxRates;
using Microsoft.Azure.Cosmos;

namespace Delivery.Domain.Services
{
    public class TaxRateService
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public TaxRateService(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<decimal> GetTaxRateAsync(string stateCode, string country)
        {
            if (country != OrderConstants.TaxRateCountry || string.IsNullOrEmpty(stateCode) || string.IsNullOrEmpty(country))
            {
                return 0;
            }
            var cosmosDatabaseAccessor = await CosmosDatabaseAccessor.CreateAsync(serviceProvider, new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider, Delivery.Domain.Constants.Constants.CosmosDatabaseHnPlatformConnectionString));
            var platformCosmosDbService = new PlatformCosmosDbService(serviceProvider, executingRequestContextAdapter, cosmosDatabaseAccessor.CosmosClient, cosmosDatabaseAccessor.GetContainer(Delivery.Domain.Constants.Constants.HnPlatform, OrderConstants.TaxRateContainer));
            
            var queryDefinition = new QueryDefinition($"SELECT d.stateName, d.stateCode, d.taxRate FROM c join d in c.data where c.partitionKey = 'UsState'");

            var taxList = await new PlatformCachedCosmosDbService(serviceProvider, executingRequestContextAdapter, platformCosmosDbService).GetItemsAsync<DocumentContract<PlatformTaxRateContract>>(queryDefinition);
            var taxRate = taxList.FirstOrDefault()?.Data.FirstOrDefault(x => x.StateCode == stateCode)?.TaxRate ?? 0;

            return taxRate;
        }
    }
}
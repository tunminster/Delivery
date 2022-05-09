using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.GeoLocations.Contracts.V1.RestContracts;
using Delivery.Domain.GeoLocations.Handlers.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexing;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreGeoUpdate
{
    public class StoreGeoUpdateCommandHandler : ICommandHandler<StoreGeoUpdateCommand, StoreGeoUpdateStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreGeoUpdateCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreGeoUpdateStatusContract> HandleAsync(StoreGeoUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StoreGeoUpdateContract.StoreId);

            var storeProperties = new[]
            {
                store.AddressLine1,
                store.AddressLine2,
                store.City,
                store.County,
                store.Country,
                store.PostalCode.Replace(" ", "")
            };
            
            var searchGeoLocationContract = new SearchGeoLocationContract
            {
                Address = string.Join("+", storeProperties.Where(x => !string.IsNullOrEmpty(x)))
            };

            var searchGeoLocationQuery = new SearchGeoLocationQuery(searchGeoLocationContract);
            var geoResult =
                await new SearchGeoLocationQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    searchGeoLocationQuery);

            store.Latitude = geoResult.Latitude;
            store.Longitude = geoResult.Longitude;
            store.FormattedAddress = geoResult.FormattedAddress;

            await databaseContext.SaveChangesAsync();

            var storeGeoUpdateStatusContract = new StoreGeoUpdateStatusContract
            {
                StoreId = store.ExternalId,
                Latitude = store.Latitude,
                Longitude = store.Longitude
            };

            // Indexing store
            await IndexStoreAsync(command.StoreGeoUpdateContract.StoreId);

            return storeGeoUpdateStatusContract;
        }

        private async Task IndexStoreAsync(string storeId)
        {
            var storeIndexCreationContract = new StoreIndexCreationContract
            {
                StoreId = storeId
            };
            
            var storeIndexStatusContract = new StoreIndexStatusContract
            {
                Status = false,
                InsertionDateTime = DateTimeOffset.UtcNow
            };


            var storeIndexCommand = new StoreIndexCommand(storeIndexCreationContract, storeIndexStatusContract);

            var storeIndexStatus = await new StoreIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(storeIndexCommand);
        }
    }
}
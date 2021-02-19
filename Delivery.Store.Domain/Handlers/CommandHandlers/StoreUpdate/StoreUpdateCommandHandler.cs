using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreUpdate
{
    public class StoreUpdateCommandHandler : ICommandHandler<StoreUpdateCommand, StoreUpdateStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreUpdateCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreUpdateStatusContract> Handle(StoreUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StoreUpdateContract.StoreId);

            var storeType =
                await databaseContext.StoreTypes.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.StoreUpdateContract.StoreTypeId);

            store.StoreName = command.StoreUpdateContract.StoreName;
            store.AddressLine1 = command.StoreUpdateContract.AddressLine1;
            store.AddressLine2 = command.StoreUpdateContract.AddressLine2;
            store.ImageUri = command.StoreUpdateContract.ImageUri;
            store.City = command.StoreUpdateContract.City;
            store.County = command.StoreUpdateContract.County;
            store.Country = command.StoreUpdateContract.Country;
            store.PostalCode = command.StoreUpdateContract.PostalCode;
            store.StoreTypeId = storeType.Id;

            foreach (var storeOpeningHour in command.StoreUpdateContract.StoreOpeningHours)
            {
                var openingHour = await databaseContext.OpeningHours.FirstOrDefaultAsync(x =>
                    x.StoreId == store.Id && x.DayOfWeek == storeOpeningHour.DayOfWeek);

                openingHour.Open = storeOpeningHour.Open;
                openingHour.Close = storeOpeningHour.Close;

                databaseContext.OpeningHours.Update(openingHour);
            }
            
            databaseContext.Stores.Update(store);
            await databaseContext.SaveChangesAsync();
            
            await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                .PublishStoreGeoUpdateMessageAsync(store.ExternalId);

            command.StoreUpdateStatusContract.StoreId = store.ExternalId;
            command.StoreUpdateStatusContract.InsertionDateTime = DateTimeOffset.UtcNow;

            return command.StoreUpdateStatusContract;
        }
    }
}
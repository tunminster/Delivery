using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopProfile
{
    public record ShopProfileUpdateCommand(ShopProfileCreationContract ShopProfileCreationContract);
    public class ShopProfileUpdateCommandHandler : ICommandHandler<ShopProfileUpdateCommand, ShopCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopProfileUpdateCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopCreationStatusContract> Handle(ShopProfileUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var storeUser = await databaseContext.StoreUsers
                .SingleOrDefaultAsync(
                    x => x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail);

            var store = await databaseContext.Stores.SingleOrDefaultAsync(x => x.Id == storeUser.StoreId);

            store.StoreTypeId = command.ShopProfileCreationContract.StoreTypeId;
            store.AddressLine1 = command.ShopProfileCreationContract.AddressLine1;
            store.AddressLine2 = command.ShopProfileCreationContract.AddressLine2;
            store.City = command.ShopProfileCreationContract.City;
            store.County = command.ShopProfileCreationContract.County;
            
            store.OpeningHours = new List<OpeningHour>();
            
            foreach (var storeOpeningHour in command.ShopProfileCreationContract.StoreOpeningHours)
            {
                store.OpeningHours.Add(new OpeningHour
                {
                    DayOfWeek = storeOpeningHour.DayOfWeek,
                    Open = storeOpeningHour.Open,
                    Close = storeOpeningHour.Close,
                    IsDeleted = false,
                    InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                    InsertionDateTime = DateTimeOffset.UtcNow
                });
            }
            
            await databaseContext.SaveChangesAsync();

            await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                .PublishStoreGeoUpdateMessageAsync(store.ExternalId);

            return new ShopCreationStatusContract
                { StoreId = store.ExternalId, ImageUri = store.ImageUri, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}
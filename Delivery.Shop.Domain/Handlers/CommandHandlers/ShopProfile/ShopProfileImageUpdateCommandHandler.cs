using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Store.Domain.Factories.GeoLocationFactory;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopProfile
{
    public record ShopProfileImageUpdateCommand(ShopProfileImageCreationContract ShopProfileImageCreationContract);
    
    public class ShopProfileImageUpdateCommandHandler : ICommandHandler<ShopProfileImageUpdateCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopProfileImageUpdateCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(ShopProfileImageUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.SingleOrDefaultAsync(x =>
                x.ExternalId == command.ShopProfileImageCreationContract.StoreId);

            if (store != null)
            {
                store.ImageUri = command.ShopProfileImageCreationContract.ImageUri;

                await databaseContext.SaveChangesAsync();
                
                await new StoreGeoLocationFactory(serviceProvider, executingRequestContextAdapter)
                    .PublishStoreGeoUpdateMessageAsync(store.ExternalId);

                return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            }
            return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}
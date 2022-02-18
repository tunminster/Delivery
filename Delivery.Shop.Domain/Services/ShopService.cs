using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Services
{
    public class ShopService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<ShopImageCreationStatusContract> UploadShopImageAsync(
            ShopImageCreationContract shopImageCreationContract)
        {
            var storeImageCreationContract = new StoreImageCreationContract
            {
                StoreId = shopImageCreationContract.StoreId,
                StoreImage = shopImageCreationContract.ShopImage,
                StoreName = shopImageCreationContract.StoreName
            };

            var storeImageCreationCommand = new StoreImageCreationCommand(storeImageCreationContract);

            var storeImageCreationStatusContract =
                await new StoreImageCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleCoreAsync(storeImageCreationCommand);

            var shopImageCreationStatusContract = new ShopImageCreationStatusContract
            {
                ShopImageUri = storeImageCreationStatusContract.ImageUri,
                DateCreated = storeImageCreationStatusContract.DateCreated
            };

            return shopImageCreationStatusContract;
        }
        
        public async Task<bool> IsShopOwnerApprovedAsync(string emailAddress)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var storeUser = await databaseContext.StoreUsers.FirstOrDefaultAsync(x => x.Username == emailAddress && x.Approved);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{nameof(IsShopOwnerApprovedAsync)} executed", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            if (storeUser is null)
            {
                return false;
            }

            return true;
        }
    }
}
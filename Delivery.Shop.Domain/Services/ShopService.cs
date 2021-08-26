using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation;

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
                    .Handle(storeImageCreationCommand);

            var shopImageCreationStatusContract = new ShopImageCreationStatusContract
            {
                ShopImageUri = storeImageCreationStatusContract.ImageUri,
                DateCreated = storeImageCreationStatusContract.DateCreated
            };

            return shopImageCreationStatusContract;
        }
    }
}
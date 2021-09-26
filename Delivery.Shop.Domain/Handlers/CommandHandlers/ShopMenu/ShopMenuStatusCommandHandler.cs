using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopMenu
{
    public record ShopMenuStatusCommand(ShopMenuStatusCreationContract ShopMenuStatusCreationContract);
    public class ShopMenuStatusCommandHandler : ICommandHandler<ShopMenuStatusCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopMenuStatusCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(ShopMenuStatusCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail 
                            ?? throw new InvalidOperationException($"Expected authenticated user email.");
            var storeUser = await databaseContext.StoreUsers.FirstOrDefaultAsync(x => x.Username == userEmail);
            
            var store = await databaseContext.Stores.FirstOrDefaultAsync(x => x.Id == storeUser.StoreId);

            if (string.IsNullOrEmpty(command.ShopMenuStatusCreationContract.ProductId))
            {
                store.IsActive = command.ShopMenuStatusCreationContract.Status;
            }
            else
            {
                var product = await databaseContext.Products.SingleOrDefaultAsync(x =>
                    x.ExternalId == command.ShopMenuStatusCreationContract.ProductId);

                if (product != null)
                {
                    product.IsActive = command.ShopMenuStatusCreationContract.Status;
                }
            }
            
            await databaseContext.SaveChangesAsync();
        }
    }
}
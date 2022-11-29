using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopApproval
{
    public record ShopApprovalCommand(ShopApprovalContract ShopApprovalContract);
    public class ShopApprovalCommandHandler : ICommandHandler<ShopApprovalCommand, ShopApprovalStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopApprovalCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopApprovalStatusContract> HandleAsync(ShopApprovalCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = await databaseContext.Stores.FirstOrDefaultAsync(x => x.ExternalId == command.ShopApprovalContract.ShopId);
            var storeUser = await databaseContext.StoreUsers.FirstOrDefaultAsync(x => x.StoreId == store.Id);

            if (store != null)
            {
                store.Approved = true;
                storeUser.Approved = true;
                await databaseContext.SaveChangesAsync();
                return new ShopApprovalStatusContract(true, DateTimeOffset.UtcNow);
            }

            return new ShopApprovalStatusContract(false, DateTimeOffset.UtcNow);
        }
    }
}
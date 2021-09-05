using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopApproval
{
    public record ShopUserApprovalCommand(ShopUserApprovalContract ShopUserApprovalContract);
    public class ShopUserApprovalCommandHandler : ICommandHandler<ShopUserApprovalCommand, ShopUserApprovalStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopUserApprovalCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopUserApprovalStatusContract> Handle(ShopUserApprovalCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.FirstOrDefaultAsync(x => x.Username == command.ShopUserApprovalContract.Email);

            if (storeUser != null)
            {
                storeUser.Approved = true;
                await databaseContext.SaveChangesAsync();

                return new ShopUserApprovalStatusContract(true, DateTimeOffset.UtcNow);
            }

            return new ShopUserApprovalStatusContract(false, DateTimeOffset.UtcNow);
        }
    }
}
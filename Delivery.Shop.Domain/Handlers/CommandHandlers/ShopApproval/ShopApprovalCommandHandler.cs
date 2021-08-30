using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopApproval
{
    public record ShopApprovalCommand(string Username);
    public class ShopApprovalCommandHandler : ICommandHandler<ShopApprovalCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopApprovalCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(ShopApprovalCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.FirstOrDefaultAsync(x => x.Username == command.Username);

            if (storeUser != null)
            {
                storeUser.Approved = true;

                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopActive
{
    public record ShopActiveCommand(ShopActiveCreationContract ShopActiveCreationContract);
    public class ShopActiveCommandHandler : ICommandHandler<ShopActiveCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopActiveCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(ShopActiveCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.Where(x => string
                    .Equals(x.Username, command.ShopActiveCreationContract.ShopUserName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefaultAsync();
            var store = await databaseContext.Stores.SingleAsync(x => x.Id == storeUser.StoreId);
            store.IsActive = command.ShopActiveCreationContract.IsActive;
            
            await databaseContext.SaveChangesAsync();
        }
    }
}
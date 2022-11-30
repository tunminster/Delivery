using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public record DeleteProductManagementCommand(string ProductId);
    
    public class DeleteProductManagementCommandHandler : ICommandHandler<DeleteProductManagementCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DeleteProductManagementCommandHandler(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter
        )
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(DeleteProductManagementCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var storeUser = await databaseContext.StoreUsers.Where(x =>
                    x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
                .SingleOrDefaultAsync();
            
            var product = await databaseContext.Products.SingleOrDefaultAsync(x =>
                x.ExternalId == command.ProductId && x.StoreId == storeUser.StoreId);

            product.IsDeleted = true;
            await databaseContext.SaveChangesAsync();
        }
    }
}
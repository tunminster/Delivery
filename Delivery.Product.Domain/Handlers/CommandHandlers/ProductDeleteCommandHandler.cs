using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public class ProductDeleteCommandHandler : ICommandHandler<ProductDeleteCommand, bool>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ProductDeleteCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> HandleAsync(ProductDeleteCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var product = await databaseContext.Products.FindAsync(command.ProductId);
            if (product == null)
            {
                return false;
            }
            
            databaseContext.Products.Remove(product);
            await databaseContext.SaveChangesAsync();
            return true;
        }
    }
}
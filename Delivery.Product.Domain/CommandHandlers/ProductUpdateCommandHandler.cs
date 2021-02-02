using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class ProductUpdateCommandHandler : ICommandHandler<ProductUpdateCommand, bool>
    {
        private readonly Delivery.Domain.Configuration.AzureStorageConfig storageConfig;
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ProductUpdateCommandHandler(AzureStorageConfig storageConfig,
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.storageConfig = storageConfig;
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> Handle(ProductUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var product = await databaseContext.Products.FirstAsync(x => x.ExternalId == command.ProductContract.Id);
            if (product == null)
            {
                return false;
            }

            var category =
                await databaseContext.Categories.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ProductContract.CategoryId);

            product.CategoryId = category.Id;
            product.Description = command.ProductContract.Description;
            product.UnitPrice = command.ProductContract.UnitPrice;
            product.ProductImage = command.ProductContract.ProductImage;
            product.ProductImageUrl = command.ProductContract.ProductImageUrl;

            databaseContext.Entry(product).State = EntityState.Modified;
            await databaseContext.SaveChangesAsync();

            return true;
        }
    }
}
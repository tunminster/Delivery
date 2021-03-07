using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Configuration;
using Delivery.Product.Domain.Contracts.V1.RestContracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public class ProductUpdateCommandHandler : ICommandHandler<ProductUpdateCommand, ProductUpdateStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ProductUpdateCommandHandler(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ProductUpdateStatusContract> Handle(ProductUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var product = await databaseContext.Products.FirstAsync(x => x.ExternalId == command.ProductUpdateContract.ProductId);

            var productUpdateStatusContract = new ProductUpdateStatusContract()
            {
                ProductId = product.ExternalId,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            if (product == null)
            {
                throw new InvalidOperationException(
                        $"{command.ProductUpdateContract.ProductId} is not existed in the system.")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
            }

            var category =
                await databaseContext.Categories.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ProductUpdateContract.CategoryId);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.ProductUpdateContract.StoreId);

            product.CategoryId = category.Id;
            product.Description = command.ProductUpdateContract.Description;
            product.UnitPrice = command.ProductUpdateContract.UnitPrice;
            product.ProductImage = command.ProductUpdateContract.ProductImage;
            product.ProductImageUrl = command.ProductUpdateContract.ProductImageUrl;
            product.StoreId = store.Id;

            databaseContext.Entry(product).State = EntityState.Modified;
            await databaseContext.SaveChangesAsync();
            

            return productUpdateStatusContract;
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public record UpdateProductManagementCommand(ProductManagementUpdateContract ProductManagementUpdateContract);
    
    public class UpdateProductManagementCommandHandler : ICommandHandler<UpdateProductManagementCommand, ProductManagementCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public UpdateProductManagementCommandHandler(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter
        )
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ProductManagementCreationStatusContract> Handle(UpdateProductManagementCommand command)
        { 
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var category =
                await databaseContext.Categories.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ProductManagementUpdateContract.CategoryId);

            var storeUser = await databaseContext.StoreUsers.Where(x =>
                    x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
                .SingleOrDefaultAsync();
            
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().Single(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());


            var product = await databaseContext.Products.SingleOrDefaultAsync(x =>
                x.ExternalId == command.ProductManagementUpdateContract.ProductId && x.StoreId == storeUser.StoreId);

            product.ProductName = command.ProductManagementUpdateContract.ProductName;
            product.Description = command.ProductManagementUpdateContract.Description;
            product.UnitPrice = command.ProductManagementUpdateContract.UnitPrice;
            product.CategoryId = category.Id;

            if (!string.IsNullOrEmpty(command.ProductManagementUpdateContract.ProductImage) &&
                !string.IsNullOrEmpty(command.ProductManagementUpdateContract.ProductImageUrl))
            {
                product.ProductImage = command.ProductManagementUpdateContract.ProductImage;
                product.ProductImageUrl = command.ProductManagementUpdateContract.ProductImageUrl;
            }
            
            await databaseContext.SaveChangesAsync();

            var productManagementCreationStatusContract = new ProductManagementCreationStatusContract
            {
                ProductId = product.ExternalId,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            return productManagementCreationStatusContract;
        }
    }
}
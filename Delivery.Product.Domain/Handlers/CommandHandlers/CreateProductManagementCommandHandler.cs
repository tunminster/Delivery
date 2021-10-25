using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public record CreateProductManagementCommand(ProductManagementCreationContract ProductManagementCreationContract, ProductManagementCreationStatusContract ProductManagementCreationStatusContract);
    
    public class CreateProductManagementCommandHandler : ICommandHandler<CreateProductManagementCommand, ProductManagementCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CreateProductManagementCommandHandler(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter
        )
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ProductManagementCreationStatusContract> Handle(CreateProductManagementCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var category =
                await databaseContext.Categories.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ProductManagementCreationContract.CategoryId);

            var storeUser = await databaseContext.StoreUsers.Where(x =>
                    x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
                .SingleOrDefaultAsync();
            
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().Single(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            
            var product = new Database.Entities.Product
            {
                ExternalId = command.ProductManagementCreationStatusContract.ProductId,
                ProductName = command.ProductManagementCreationContract.ProductName,
                Description = command.ProductManagementCreationContract.Description,
                UnitPrice = command.ProductManagementCreationContract.UnitPrice,
                CategoryId = category.Id,
                Currency = shardInformation.Currency,
                CurrencySign = shardInformation.Currency?.ToLower() == CurrencySign.Usd.ToString()?.ToLower() ? CurrencySign.Usd.Code : CurrencySign.BritishPound.Code,
                StoreId =  storeUser.StoreId,
                ProductImage = command.ProductManagementCreationContract.ProductImage,
                ProductImageUrl = command.ProductManagementCreationContract.ProductImageUrl,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            await databaseContext.Products.AddAsync(product);
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
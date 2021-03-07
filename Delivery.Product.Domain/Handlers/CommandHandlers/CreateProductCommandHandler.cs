using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Database.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CreateProductCommandHandler(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter
            )
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<ProductCreationStatusContract> Handle(CreateProductCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var category =
                await databaseContext.Categories.FirstOrDefaultAsync(x =>
                    x.ExternalId == command.ProductCreationContract.CategoryId);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.ProductCreationContract.StoreId);
            
            var product = new Database.Entities.Product
            {
                ProductName = command.ProductCreationContract.ProductName,
                Description = command.ProductCreationContract.Description,
                UnitPrice = command.ProductCreationContract.UnitPrice,
                CategoryId = category.Id,
                Currency = Currency.BritishPound.ToString(),
                CurrencySign = CurrencySign.BritishPound.Code,
                StoreId =  store.Id,
                ProductImage = command.ProductCreationContract.ProductImage,
                ProductImageUrl = command.ProductCreationContract.ProductImageUrl,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            await databaseContext.Products.AddAsync(product);
            await databaseContext.SaveChangesAsync();

            var productCreationStatusContract = new ProductCreationStatusContract
            {
                ProductId = command.ProductId,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            return productCreationStatusContract;
        }
        
    }
}
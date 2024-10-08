using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.Handlers.QueryHandlers
{
    public class ProductByIdQueryHandler : IQueryHandler<ProductByIdQuery, ProductContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ProductByIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ProductContract> Handle(ProductByIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var product = await databaseContext.Products
                .Include(x => x.MeatOptions)
                .ThenInclude(x => x.MeatOptionValues)
                .Include(x => x.Category)
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.ExternalId == query.ProductId);
            var productContract = new ProductContract
            {
                Id = product.ExternalId,
                CategoryId = product.Category.ExternalId,
                CategoryName = product.Category.CategoryName,
                Description = product.Description,
                ProductImage = product.ProductImage,
                ProductImageUrl = product.ProductImageUrl,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                StoreId = product.Store.ExternalId,
                ProductMeatOptions = product.MeatOptions?.Select(x => x.ConvertToProductMeatOptionContract()).ToList()
            };
            return productContract;
        }
    }
}
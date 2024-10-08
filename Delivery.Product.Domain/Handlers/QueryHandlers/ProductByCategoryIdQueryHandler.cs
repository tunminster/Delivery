using System;
using System.Collections.Generic;
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
    public class ProductByCategoryIdQueryHandler : IQueryHandler<ProductByCategoryIdQuery, List<ProductContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ProductByCategoryIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<ProductContract>> Handle(ProductByCategoryIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var category = await databaseContext.Categories.FirstOrDefaultAsync(x => x.ExternalId == query.CategoryId);
            
            var productList = await databaseContext.Products.Where(x => x.CategoryId == category.Id)
                .Include(x => x.MeatOptions)
                .ThenInclude(x => x.MeatOptionValues)
                .Include(x => x.Category)
                .Include(x => x.Store)
                .Select(x => new ProductContract
                {
                    Id = x.ExternalId,
                    CategoryId = x.Category.ExternalId,
                    CategoryName = x.Category.CategoryName,
                    Description = x.Description,
                    ProductName = x.ProductName,
                    ProductImage = x.ProductImage,
                    ProductImageUrl = x.ProductImageUrl,
                    UnitPrice = x.UnitPrice,
                    StoreId = x.Store.ExternalId,
                    ProductMeatOptions = x.MeatOptions.Select(mt => mt.ConvertToProductMeatOptionContract()).ToList()
                })
                .ToListAsync();
            
            return productList;
        }
    }
}
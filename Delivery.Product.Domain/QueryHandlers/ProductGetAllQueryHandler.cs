using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductGetAllQueryHandler : IQueryHandler<ProductGetAllQuery, List<ProductContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ProductGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async  Task<List<ProductContract>> Handle(ProductGetAllQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var productContractList =  await databaseContext.Products
                .Include(x => x.Category)
                .Include(x => x.Store)
                .Select(x => new ProductContract
                {
                    Id = x.ExternalId,
                    CategoryName = x.Category.CategoryName,
                    CategoryId = x.Category.ExternalId,
                    Description = x.Description,
                    ProductName = x.ProductName,
                    ProductImage = x.ProductImage,
                    ProductImageUrl = x.ProductImageUrl,
                    UnitPrice = x.UnitPrice,
                    StoreId = x.Store.ExternalId 
                }).ToListAsync();

            return productContractList;
        }
    }
}
using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Product.Domain.QueryHandlers
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
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.ExternalId == query.ProductId);
            var productContract = new ProductContract
            {
                Id = product.ExternalId,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.CategoryName,
                Description = product.Description,
                ProductImage = product.ProductImage,
                ProductImageUrl = product.ProductImageUrl,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice
            };
            return productContract;
        }
    }
}
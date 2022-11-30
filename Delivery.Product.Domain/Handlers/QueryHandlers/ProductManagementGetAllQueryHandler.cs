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
    public record ProductManagementGetAllQuery : IQuery<List<ProductContract>>;
    public class ProductManagementGetAllQueryHandler : IQueryHandler<ProductManagementGetAllQuery, List<ProductContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ProductManagementGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<List<ProductContract>> Handle(ProductManagementGetAllQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.Where(x =>
                    x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
                .SingleOrDefaultAsync();
            
            var productContractList =  await databaseContext.Products
                .Where(x => x.StoreId == storeUser.StoreId && x.IsDeleted == false)
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
                    StoreId = x.Store.ExternalId,
                    ProductMeatOptions = x.MeatOptions.Select(mt => mt.ConvertToProductMeatOptionContract()).ToList()
                }).ToListAsync();


            return productContractList;
        }
    }
}
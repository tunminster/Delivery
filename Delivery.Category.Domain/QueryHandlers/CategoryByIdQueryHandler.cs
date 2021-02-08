using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByIdQueryHandler: IQueryHandler<CategoryByIdQuery, CategoryContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CategoryByIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CategoryContract> Handle(CategoryByIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var category = await databaseContext.Categories.Include(x => x.Store).FirstOrDefaultAsync(x => x.ExternalId == query.CategoryId);

            var categoryContract = new CategoryContract
            {
                Id = category.ExternalId,
                StoreId = category.Store.ExternalId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                Order = category.Order,
                ParentCategoryId = category.ParentCategoryId
            };

            return categoryContract;
        }
    }
}
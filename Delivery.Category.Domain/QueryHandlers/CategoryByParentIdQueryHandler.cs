using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByParentIdQueryHandler :IQueryHandler<CategoryByParentIdQuery, List<CategoryContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CategoryByParentIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<CategoryContract>> Handle(CategoryByParentIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var parentCategory = await databaseContext.Categories.FirstOrDefaultAsync(x => x.ExternalId == query.ParentId);

            var parentCategoryId = parentCategory.Id;
            
            var result = await databaseContext.Categories
                .Where(x => x.ParentCategoryId == parentCategoryId)
                .Select(x => new CategoryContract()
                {
                    Id = x.ExternalId,
                    CategoryName = x.CategoryName,
                    Description = x.Description,
                    Order = x.Order,
                    ParentCategoryId = x.ParentCategoryId
                })
                .ToListAsync();

            return result;
        }
    }
}
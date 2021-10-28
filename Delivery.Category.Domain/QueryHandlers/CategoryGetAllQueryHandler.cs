using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CategoryGetAllQueryHandler : IQueryHandler<CategoryGetAllQuery, List<CategoryContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CategoryGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        public async Task<List<CategoryContract>> Handle(CategoryGetAllQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var categoryContractList =  await databaseContext.Categories.Where(x => x.IsDeleted != true).Include(x => x.Store)
                .Select(x => new CategoryContract()
            {
                Id = x.ExternalId,
                CategoryName = x.CategoryName,
                StoreId = x.Store.ExternalId,
                Description = x.Description,
                Order = x.Order,
                ParentCategoryId = x.ParentCategoryId
            }).ToListAsync();
            
            return categoryContractList;
        }
    }
}
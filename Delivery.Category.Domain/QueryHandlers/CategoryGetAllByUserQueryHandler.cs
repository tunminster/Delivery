using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryGetAllByUserQuery : IQuery<List<CategoryContract>>
    {
        public string Email { get; init; } = string.Empty;
    }
    
    public class CategoryGetAllByUserQueryHandler : IQueryHandler<CategoryGetAllByUserQuery, List<CategoryContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CategoryGetAllByUserQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<CategoryContract>> Handle(CategoryGetAllByUserQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var user = executingRequestContextAdapter.GetAuthenticatedUser();
            var storeUser = await databaseContext.StoreUsers.FirstAsync(x => x.Username == user.UserEmail);
            var categoryContractList =  await databaseContext.Categories.Where(x => x.StoreId == storeUser.StoreId).Include(x => x.Store)
                .Select(x => new CategoryContract
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
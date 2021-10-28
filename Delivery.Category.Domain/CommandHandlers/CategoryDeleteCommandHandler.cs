using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand, CategoryUpdateStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CategoryDeleteCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CategoryUpdateStatusContract> Handle(CategoryDeleteCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var user = executingRequestContextAdapter.GetAuthenticatedUser();
            var storeUser = await databaseContext.StoreUsers.SingleAsync(x => x.Username == user.UserEmail);
            
            var category = await databaseContext.Categories.SingleAsync(x => x.ExternalId == command.CategoryId && x.StoreId == storeUser.StoreId);
            var categoryUpdateStatusContract = new CategoryUpdateStatusContract();
            
            if (category == null)
            {
                return categoryUpdateStatusContract;
            }

            category.IsDeleted = true;
            
            //databaseContext.Categories.Remove(category);
            await databaseContext.SaveChangesAsync();
            categoryUpdateStatusContract.Updated = true;

            return categoryUpdateStatusContract;
        }
    }
}
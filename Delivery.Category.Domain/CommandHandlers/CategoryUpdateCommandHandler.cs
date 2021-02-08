using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommandHandler : ICommandHandler<CategoryUpdateCommand, CategoryUpdateStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CategoryUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CategoryUpdateStatusContract> Handle(CategoryUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var category = await databaseContext.Categories.FirstAsync(x => x.ExternalId == command.CategoryCreationContract.Id);

            var categoryUpdateStatusContract = new CategoryUpdateStatusContract();
            if (category == null)
            {
                return categoryUpdateStatusContract;
            }

            category.Description = command.CategoryCreationContract.Description;
            category.Order = command.CategoryCreationContract.Order;
            category.CategoryName = command.CategoryCreationContract.CategoryName;
            category.ParentCategoryId = command.CategoryCreationContract.ParentCategoryId;
            
            databaseContext.Entry(category).State = EntityState.Modified;
            await databaseContext.SaveChangesAsync();
            categoryUpdateStatusContract.Updated = true;

            return categoryUpdateStatusContract;
        }
    }
}
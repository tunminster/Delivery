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
    public class CategoryCreationCommandHandler : ICommandHandler<CategoryCreationCommand, CategoryCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CategoryCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CategoryCreationStatusContract> Handle(CategoryCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.FirstOrDefaultAsync(x =>
                x.ExternalId == command.CategoryCreationContract.StoreId);
            var category = new Database.Entities.Category
            {
                CategoryName = command.CategoryCreationContract.CategoryName,
                StoreId = store.Id,
                Description = command.CategoryCreationContract.Description,
                ParentCategoryId = command.CategoryCreationContract.ParentCategoryId,
                Order = command.CategoryCreationContract.Order
            };

            await databaseContext.Categories.AddAsync(category);
            await databaseContext.SaveChangesAsync();

            var categoryCreationStatusContract = new CategoryCreationStatusContract {Published = true};

            return categoryCreationStatusContract;
        }
    }
}
using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

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
            
            var category = new Database.Entities.Category
            {
                CategoryName = command.CategoryContract.CategoryName,
                Description = command.CategoryContract.Description,
                ParentCategoryId = command.CategoryContract.ParentCategoryId,
                Order = command.CategoryContract.Order
            };

            await databaseContext.Categories.AddAsync(category);
            await databaseContext.SaveChangesAsync();

            var categoryCreationStatusContract = new CategoryCreationStatusContract {Published = true};

            return categoryCreationStatusContract;
        }
    }
}
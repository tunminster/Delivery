using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

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
            
            var category = await databaseContext.Categories.FindAsync(command.CategoryId);
            var categoryUpdateStatusContract = new CategoryUpdateStatusContract();
            
            if (category == null)
            {
                return categoryUpdateStatusContract;
            }
            
            databaseContext.Categories.Remove(category);
            await databaseContext.SaveChangesAsync();
            categoryUpdateStatusContract.Updated = true;

            return categoryUpdateStatusContract;
        }
    }
}
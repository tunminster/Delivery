using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand, CategoryUpdateStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        
        public CategoryDeleteCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<CategoryUpdateStatusContract> Handle(CategoryDeleteCommand command)
        {
            var category = await applicationDbContext.Categories.FindAsync(command.CategoryId);
            var categoryUpdateStatusContract = new CategoryUpdateStatusContract();
            
            if (category == null)
            {
                return categoryUpdateStatusContract;
            }
            
            applicationDbContext.Categories.Remove(category);
            await applicationDbContext.SaveChangesAsync();
            categoryUpdateStatusContract.Updated = true;

            return categoryUpdateStatusContract;
        }
    }
}
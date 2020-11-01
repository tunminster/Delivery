using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommandHandler : ICommandHandler<CategoryUpdateCommand, CategoryUpdateStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        
        public CategoryUpdateCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<CategoryUpdateStatusContract> Handle(CategoryUpdateCommand command)
        {
            var category = await applicationDbContext.Categories.FindAsync(command.CategoryContract.Id);

            var categoryUpdateStatusContract = new CategoryUpdateStatusContract();
            if (category == null)
            {
                return categoryUpdateStatusContract;
            }

            category.Description = command.CategoryContract.Description;
            category.Order = command.CategoryContract.Order;
            category.CategoryName = command.CategoryContract.CategoryName;
            category.ParentCategoryId = command.CategoryContract.ParentCategoryId;
            
            applicationDbContext.Entry(category).State = EntityState.Modified;
            await applicationDbContext.SaveChangesAsync();
            categoryUpdateStatusContract.Updated = true;

            return categoryUpdateStatusContract;
        }
    }
}
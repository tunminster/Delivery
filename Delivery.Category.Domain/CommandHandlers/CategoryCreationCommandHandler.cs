using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryCreationCommandHandler : ICommandHandler<CategoryCreationCommand, CategoryCreationStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        
        public CategoryCreationCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<CategoryCreationStatusContract> Handle(CategoryCreationCommand command)
        {
            var category = new Database.Entities.Category
            {
                CategoryName = command.CategoryContract.CategoryName,
                Description = command.CategoryContract.Description,
                ParentCategoryId = command.CategoryContract.ParentCategoryId,
                Order = command.CategoryContract.Order
            };

            applicationDbContext.Categories.Add(category);
            await applicationDbContext.SaveChangesAsync();

            var categoryCreationStatusContract = new CategoryCreationStatusContract {Published = true};

            return categoryCreationStatusContract;
        }
    }
}
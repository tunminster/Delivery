using Delivery.Category.Domain.Contracts;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommand
    {
        public CategoryUpdateCommand(CategoryContract categoryContract)
        {
            CategoryContract = categoryContract;
        }
        public CategoryContract CategoryContract { get; } 
    }
}
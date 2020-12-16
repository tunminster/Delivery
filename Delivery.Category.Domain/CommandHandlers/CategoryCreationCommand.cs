using Delivery.Category.Domain.Contracts;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryCreationCommand 
    {
        public CategoryCreationCommand(CategoryContract categoryContract)
        {
            CategoryContract = categoryContract;
        }
        
        public CategoryContract CategoryContract { get; }
    }
    
    
}
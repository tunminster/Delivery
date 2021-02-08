using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryCreationCommand 
    {
        public CategoryCreationCommand(CategoryCreationContract categoryCreationContract)
        {
            CategoryCreationContract = categoryCreationContract;
        }
        
        public CategoryCreationContract CategoryCreationContract { get; }
    }
    
    
}
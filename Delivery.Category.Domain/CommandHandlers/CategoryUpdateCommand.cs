using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommand
    {
        public CategoryUpdateCommand(CategoryCreationContract categoryCreationContract)
        {
            CategoryCreationContract = categoryCreationContract;
        }
        public CategoryCreationContract CategoryCreationContract { get; } 
    }
}
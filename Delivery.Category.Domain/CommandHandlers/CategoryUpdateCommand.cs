using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommand
    {
        public CategoryUpdateCommand(CategoryCreationContract categoryCreationContract, string id)
        {
            CategoryCreationContract = categoryCreationContract;
            Id = id;
        }
        public CategoryCreationContract CategoryCreationContract { get; } 
        
        public string Id { get; }
    }
}
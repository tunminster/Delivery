using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryCreationCommandHandler : ICommandHandler<CategoryCreationCommand, CategoryCreationStatusContract>
    {
        public Task<CategoryCreationStatusContract> Handle(CategoryCreationCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
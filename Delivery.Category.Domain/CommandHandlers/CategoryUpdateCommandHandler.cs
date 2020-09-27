using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryUpdateCommandHandler : ICommandHandler<CategoryUpdateCommand, CategoryUpdateStatusContract>
    {
        public Task<CategoryUpdateStatusContract> Handle(CategoryUpdateCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
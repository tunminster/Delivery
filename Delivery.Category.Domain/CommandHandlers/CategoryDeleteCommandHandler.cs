using System.Threading.Tasks;
using Delivery.Category.Domain.Contracts;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Category.Domain.CommandHandlers
{
    public class CategoryDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand, CategoryUpdateStatusContract>
    {
        public Task<CategoryUpdateStatusContract> Handle(CategoryDeleteCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
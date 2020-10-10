using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressDeleteCommandHandler : ICommandHandler<AddressDeleteCommand, AddressDeleteStatusContract>
    {
        public Task<AddressDeleteStatusContract> Handle(AddressDeleteCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
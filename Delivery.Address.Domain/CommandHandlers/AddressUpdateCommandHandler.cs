using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressUpdateCommandHandler : ICommandHandler<AddressUpdateCommand, AddressUpdateStatusContract>
    {
        public Task<AddressUpdateStatusContract> Handle(AddressUpdateCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
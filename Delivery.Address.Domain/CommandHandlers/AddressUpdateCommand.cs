using Delivery.Address.Domain.Contracts;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressUpdateCommand
    {
        public AddressUpdateCommand(AddressContract addressContract)
        {
            AddressContract = addressContract;
        }
        public AddressContract AddressContract { get; }
    }
}
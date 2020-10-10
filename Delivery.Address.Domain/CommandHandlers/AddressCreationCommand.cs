using Delivery.Address.Domain.Contracts;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressCreationCommand
    {

        public AddressCreationCommand(AddressContract addressContract)
        {
            AddressContract = addressContract;
        }

        public AddressContract AddressContract { get; }
    }
}
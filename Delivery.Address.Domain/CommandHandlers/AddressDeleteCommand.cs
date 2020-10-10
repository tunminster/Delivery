namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressDeleteCommand
    {
        public AddressDeleteCommand(int addressId)
        {
            AddressId = addressId;
        }
        public int AddressId { get; }
    }
}
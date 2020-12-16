namespace Delivery.Address.Domain.Contracts
{
    public class AddressCreationStatusContract
    {
        public AddressCreationStatusContract(bool isAddressSaved)
        {
            IsAddressSaved = isAddressSaved;
        }
        public bool IsAddressSaved { get; }
    }
}
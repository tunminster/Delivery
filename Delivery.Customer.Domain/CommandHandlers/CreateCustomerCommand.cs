using Delivery.Customer.Domain.Contracts.RestContracts;

namespace Delivery.Customer.Domain.CommandHandlers
{
    public class CreateCustomerCommand
    {
        public CreateCustomerCommand(CustomerCreationContract customerCreationContract)
        {
            CustomerCreationContract = customerCreationContract;
        }
        public CustomerCreationContract CustomerCreationContract { get; }
    }
}
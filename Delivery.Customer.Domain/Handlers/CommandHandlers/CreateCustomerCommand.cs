using Delivery.Customer.Domain.Contracts.V1.RestContracts;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
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
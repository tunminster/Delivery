using Delivery.Customer.Domain.Contracts.V1.RestContracts;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
{
    public class UpdateCustomerCommand
    {
        public UpdateCustomerCommand(CustomerUpdateContract customerUpdateContract)
        {
            CustomerUpdateContract = customerUpdateContract;
        }
        public CustomerUpdateContract CustomerUpdateContract { get; }
    }
}
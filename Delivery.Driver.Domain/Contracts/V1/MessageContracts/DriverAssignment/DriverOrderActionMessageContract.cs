using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment
{
    public class DriverOrderActionMessageContract : AuditableResponseMessage<DriverOrderActionContract, StatusContract>
    {
    }
}
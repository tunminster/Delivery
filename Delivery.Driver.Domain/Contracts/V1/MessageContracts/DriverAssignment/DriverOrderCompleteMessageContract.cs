using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment
{
    public class DriverOrderCompleteMessageContract : AuditableResponseMessage<EntityUpdateContract, StatusContract>
    {
    }
}
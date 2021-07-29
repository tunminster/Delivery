using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts
{
    public class DriverCreationMessageContract : AuditableResponseMessage<DriverCreationContract, DriverCreationStatusContract>
    {
    }
}
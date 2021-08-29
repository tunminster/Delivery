using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverActive
{
    public class DriverActiveMessageContract : AuditableResponseMessage<DriverActiveCreationContract, DriverActiveStatusContract>
    {
    }
}
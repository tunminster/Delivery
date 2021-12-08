using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverIndex;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverIndex
{
    public class DriverIndexMessageContract : AuditableResponseMessage<DriverIndexCreationContract, StatusContract>
    {
        
    }
}
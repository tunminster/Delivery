using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection
{
    public class DriverOrderRejectionMessageContract : AuditableResponseMessage<DriverOrderRejectionContract, StatusContract>
    {
        
    }
}
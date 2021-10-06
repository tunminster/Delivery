using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts
{
    public class OrderIndexMessageContract : AuditableResponseMessage<IndexCreationContract, StatusContract>
    {
    }
}
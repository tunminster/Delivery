using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts
{
    [DataContract]
    public class OrderUpdateMessage : AuditableResponseMessage<StripeOrderUpdateContract, StripeOrderUpdateStatusContract>
    {
        
    }
}
using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts
{
    [DataContract]
    public class OrderCreationMessage : AuditableResponseMessage<StripeOrderCreationContract, OrderCreationStatusContract>
    {
        
    }
}
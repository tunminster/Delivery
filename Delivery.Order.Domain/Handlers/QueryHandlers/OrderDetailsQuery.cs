using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Domain.Enum;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts.V1.RestContracts.OrderDetails;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public class OrderDetailsQuery : IQuery<OrderDetailsContract>
    {
        public OrderDetailsQuery(string orderId, int deliveryTimeZone)
        {
            OrderId = orderId;
            DeliveryTimeZone = (DeliveryTimeZone)deliveryTimeZone;
        }
        public string OrderId { get; private set; }
        
        public DeliveryTimeZone DeliveryTimeZone { get; private set; }
    }
}
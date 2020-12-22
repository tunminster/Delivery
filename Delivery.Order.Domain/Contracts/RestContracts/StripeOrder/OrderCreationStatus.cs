using System;
using System.Runtime.Serialization;
using Microsoft.Graph;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrder
{
    public class OrderCreationStatus
    {
        public OrderCreationStatus(string orderId, int totalAmount, DateTimeOffset createdDateTime)
        {
            OrderId = orderId;
            TotalAmount = totalAmount;
            CreatedDateTime = createdDateTime;
        }
        public string OrderId { get; }
        public int TotalAmount { get; }
        public DateTimeOffset CreatedDateTime { get; }
    }
}
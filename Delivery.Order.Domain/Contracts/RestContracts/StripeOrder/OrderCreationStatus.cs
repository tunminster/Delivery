using System;
using System.Runtime.Serialization;
using Microsoft.Graph;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrder
{
    public class OrderCreationStatus
    {
        public string OrderId { get; set; }
        public int TotalAmount { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
    }
}
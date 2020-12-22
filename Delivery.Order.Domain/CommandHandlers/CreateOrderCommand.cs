using System;
using System.Collections.Generic;

namespace Delivery.Order.Domain.CommandHandlers
{
    public class CreateOrderCommand
    {
        public CreateOrderCommand()
        {
            OrderItems = new List<OrderItemCommand>();
        }

        public string Description { get; set; }

        public int TotalAmount { get; set; }

        public string CurrencyCode { get; set; }

        public string PaymentType { get; set; }

        public string PaymentCard { get; set; }

        public string PaymentStatus { get; set; }

        public string PaymentExpiryMonth { get; set; }

        public string PaymentExpiryYear { get; set; }

        public string PaymentCVC { get; set; }

        public string PaymentIssueNumber { get; set; }

        public string OrderStatus { get; set; }

        public string CardHolderName { get; set; }

        public int CustomerId { get; set; }

        public DateTime DateCreated { get; set; }

        public IList<OrderItemCommand> OrderItems { get; set; }

        public int ShippingAddressId { get; set; }

        public bool SaveCard { get; set; }
    }

    public class OrderItemCommand
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
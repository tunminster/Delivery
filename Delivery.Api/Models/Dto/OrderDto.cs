using System;

namespace Delivery.Api.Models.Dto
{
    public class OrderDto
    {
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string Cvc { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string TotalAmount { get; set; }
        public int CustomerId { get; set; }
        public OrderItemDto[] OrderItems { get; set; }
        public int ShippingAddressId { get; set; }
        public bool SaveCard { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set;}
        public int Count { get; set; }
    }
}

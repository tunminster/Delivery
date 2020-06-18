using System;
namespace Delivery.Api.Models.Dto
{
    public class OrderViewDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public OrderItemDto[] OrderItems { get; set; }
    }
}

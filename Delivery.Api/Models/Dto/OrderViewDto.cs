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
        public OrderViewItemDto[] OrderItems { get; set; }
    }

    public class OrderViewItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Count { get; set; }
    }
}

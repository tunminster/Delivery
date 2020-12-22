using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Database.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }

        public int TotalAmount { get; set; }

        [MaxLength(15)]
        public string CurrencyCode { get; set; }

        [MaxLength(15)]
        public string PaymentType { get; set; }

        [MaxLength(25)]
        public string PaymentCard { get; set; }

        [MaxLength(15)]
        public string PaymentStatus { get; set; }

        [MaxLength(15)]
        public string OrderStatus { get; set; }

        [MaxLength(50)]
        public string PaymentOrderCodeRef { get; set; }

        public int CustomerId { get; set; }

        public DateTime DateCreated { get; set; }

        public int AddressId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
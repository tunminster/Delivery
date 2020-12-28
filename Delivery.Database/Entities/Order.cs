using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class Order : Entity
    {

        [MaxLength(300)]
        public string Description { get; set; }

        public int TotalAmount { get; set; }

        [MaxLength(15)]
        public string CurrencyCode { get; set; }

        [MaxLength(15)]
        public string PaymentType { get; set; }
        

        [MaxLength(15)]
        public string PaymentStatus { get; set; }

        [MaxLength(15)]
        public string OrderStatus { get; set; }

        [MaxLength(50)]
        public string PaymentIntentId { get; set; }

        public int CustomerId { get; set; }

        public DateTime DateCreated { get; set; }

        public int AddressId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
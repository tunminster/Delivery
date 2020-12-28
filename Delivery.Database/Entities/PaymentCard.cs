using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Database.Entities.V1;

namespace Delivery.Database.Entities
{
    public class PaymentCard : Entity
    {

        [MaxLength(1000)]
        public string Token { get; set; }

        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(30)]
        public string CardType { get; set; }

        [MaxLength(30)]
        public string MaskedCardNumber { get; set; }

        [MaxLength(10)]
        public string ExpiryMonth { get; set; }

        [MaxLength(10)]
        public string ExpiryYear { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
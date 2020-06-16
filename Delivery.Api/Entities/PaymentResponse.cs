using System;
using System.ComponentModel.DataAnnotations;

namespace Delivery.Api.Entities
{
    public class PaymentResponse
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(250)]
        public string OrderCode { get; set; }

        [MaxLength(250)]
        public string Token { get; set; }

        [MaxLength(250)]
        public string OrderDescription { get; set; }

        [MaxLength(20)]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; }

        [MaxLength(10)]
        public string PaymentStatus { get; set; }

        [MaxLength(30)]
        public string MaskedCardNumber { get; set; }

        [MaxLength(10)]
        public string CvcResultCode { get; set; }

        [MaxLength(10)]
        public string Environment { get; set; }

        public DateTime DateAdded { get; set; }
    }
}

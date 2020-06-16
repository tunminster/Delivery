using System;
namespace Delivery.Api.Domain.Command
{
    public class CreateDirectPaymentCommand
    {
        public string Name { get; set; }

        public string CardType { get; set; }

        public string CardNumber { get; set; }

        public string ExpiryMonth { get; set; }

        public string ExpiryYear { get; set; }

        public int CustomerId { get; set; }

        public string OrderType { get; set; }

        public string OrderDescription { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; }
    }
}

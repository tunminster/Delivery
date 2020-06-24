using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Delivery.Api.Models
{
    
    public class WorldPayPaymentModel
    {
        public WorldPayPaymentModel()
        {
            paymentMethod = new PaymentMethod();
        }

        [JsonProperty("paymentMethod")]
        public PaymentMethod paymentMethod{get; set;}

        [JsonProperty("orderType")]
        public string OrderType { get; set; }

        [JsonProperty("orderDescription")]
        public string OrderDescription { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("reusable")]
        public bool Reusable { get; set; }
    }

    public class PaymentMethod
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("expiryMonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiryYear")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("cvc")]
        public string Cvc { get; set; }

        [JsonProperty("issueNumber")]
        public string IssueNumber { get; set; }
    }
}

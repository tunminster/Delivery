using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Delivery.Api.Models
{
    public class WorldPayPaymentResponseModel
    {
        [JsonProperty("orderCode")]
        public string OrderCode { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("orderDescription")]
        public string OrderDescription { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty("paymentResponse")]
        public PaymentResponse PaymentResponse { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("riskScore")]
        public RiskScore RiskScore {get; set;}

        [JsonProperty("resultCodes")]
        public ResultCodes ResultCodes { get; set; }
    }

    public class PaymentResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("expiryMonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiryYear")]
        public string ExpiryYear { get; set; }

        [JsonProperty("issueNumber")]
        public string IssueNumber { get; set; }

        [JsonProperty("cardType")]
        public string CardType { get; set; }

        [JsonProperty("maskedCardNumber")]
        public string MaskedCardNumber { get; set; }

        [JsonProperty("cardSchemaType")]
        public string CardSchemaType { get; set; }

        [JsonProperty("cardSchemaName")]
        public string CardSchemaName { get; set; }

        [JsonProperty("cardIssuer")]
        public string CardIssuer { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("cardClass")]
        public string CardClass { get; set; }

        [JsonProperty("cardProductTypeDescNonContactless")]
        public string CardProductTypeDescNonContactless { get; set; }

        [JsonProperty("cardProductTypeDescContactless")]
        public string CardProductTypeDescContactless { get; set; }

        [JsonProperty("prepaid")]
        public string Prepaid { get; set; }
    }

    public class RiskScore
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ResultCodes
    {
        [JsonProperty("avsResultCode")]
        public string AvsResultCode { get; set; }

        [JsonProperty("cvcResultCode")]
        public string CvcResultCode { get; set; }
    }
}

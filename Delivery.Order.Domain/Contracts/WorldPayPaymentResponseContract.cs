using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts
{
    [DataContract]
    public class WorldPayPaymentResponseContract
    {
        [DataMember]
        public string OrderCode { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string OrderDescription { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public string PaymentStatus { get; set; }

        [DataMember]
        public PaymentResponse PaymentResponse { get; set; }

        [DataMember]
        public string Environment { get; set; }

        [DataMember]
        public RiskScore RiskScore {get; set;}

        [DataMember]
        public ResultCodes ResultCodes { get; set; }
    }
    
    [DataContract]
    public class PaymentResponse
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ExpiryMonth { get; set; }

        [DataMember]
        public string ExpiryYear { get; set; }

        [DataMember]
        public string IssueNumber { get; set; }

        [DataMember]
        public string CardType { get; set; }

        [DataMember]
        public string MaskedCardNumber { get; set; }

        [DataMember]
        public string CardSchemaType { get; set; }

        [DataMember]
        public string CardSchemaName { get; set; }

        [DataMember]
        public string CardIssuer { get; set; }

        [DataMember]
        public string CountryCode { get; set; }

        [DataMember]
        public string CardClass { get; set; }

        [DataMember]
        public string CardProductTypeDescNonContactless { get; set; }

        [DataMember]
        public string CardProductTypeDescContactless { get; set; }

        [DataMember]
        public string Prepaid { get; set; }
    }

    [DataContract]
    public class RiskScore
    {
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class ResultCodes
    {
        [DataMember]
        public string AvsResultCode { get; set; }

        [DataMember]
        public string CvcResultCode { get; set; }
        
    }
}
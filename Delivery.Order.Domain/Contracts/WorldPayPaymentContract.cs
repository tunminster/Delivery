using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts
{
    [DataContract]
    public class WorldPayPaymentContract
    {
        public WorldPayPaymentContract()
        {
            PaymentMethod = new PaymentMethod();
        }

        [DataMember]
        public PaymentMethod PaymentMethod{get; set;}

        [DataMember]
        public string OrderType { get; set; }

        [DataMember]
        public string OrderDescription { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public bool Reusable { get; set; }
    }
    
    [DataContract]
    public class PaymentMethod
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
        public string CardNumber { get; set; }

        [DataMember]
        public string Cvc { get; set; }

        [DataMember]
        public string IssueNumber { get; set; }
    }
}
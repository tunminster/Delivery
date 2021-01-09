using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments
{
    /// <summary>
    ///  A contract represents status of a stripe payment creation
    /// </summary>
    [DataContract]
    public class StripePaymentCreationStatusContract
    {
        [DataMember]
        public string StripePaymentId { get; set; }
        
        [DataMember]
        public DateTimeOffset DateCreated { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StripePaymentId)}: {StripePaymentId.Format()}," +
                   $"{nameof(DateCreated)}: {DateCreated.Format()};";
        }
    }
}
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class StripeAccountLinkCreationContract
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string RefreshUrl { get;  set; }
        
        [DataMember]
        public string ReturnUrl { get; set; }
        
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()}," +
                   $"{nameof(RefreshUrl)} : {RefreshUrl.Format()}," +
                   $"{nameof(ReturnUrl)} : {ReturnUrl.Format()};";
        }
    }
}
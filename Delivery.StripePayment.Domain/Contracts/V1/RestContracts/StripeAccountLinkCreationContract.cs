using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class StripeAccountLinkCreationContract
    {
        [DataMember] public string AccountId { get; set; } = string.Empty;

        [DataMember] public string RefreshUrl { get; set; } = string.Empty;

        [DataMember] public string ReturnUrl { get; set; } = string.Empty;
        
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()}," +
                   $"{nameof(RefreshUrl)} : {RefreshUrl.Format()}," +
                   $"{nameof(ReturnUrl)} : {ReturnUrl.Format()};";
        }
    }
}
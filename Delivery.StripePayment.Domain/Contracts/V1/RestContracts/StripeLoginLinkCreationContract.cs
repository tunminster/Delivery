using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents to create a stripe login link.
    ///  This login link is only for express account. https://stripe.com/docs/api/account/create_login_link
    /// </summary>
    [DataContract]
    public class StripeLoginLinkCreationContract
    {
        [DataMember]
        public string AccountId { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()};";

        }
    }
}
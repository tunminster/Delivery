using System.Runtime.Serialization;

namespace Delivery.StripePayment.Domain.Contracts.Enums
{
    /// <summary>
    ///  Stripe Account Type
    /// </summary>
    [DataContract]
    public enum StripeAccountType
    {
        [EnumMember] None = 0,
        [EnumMember] Standard = 1,
        [EnumMember] Express = 2
        
    }
}
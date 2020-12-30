using System.Runtime.Serialization;

namespace Delivery.StripePayment.Domain.Contracts.Enums
{
    /// <summary>
    ///  An enum represents stripe payment capture status
    /// </summary>
    [DataContract]
    public enum StripePaymentCaptureStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Confirmed = 1,
        [EnumMember] Succeeded = 2,
        [EnumMember] Failed =3
    }
}